using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Commons.Environment;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreApplication{TStartup}"/> is the abstract class developers inherit from in order to create Vostok-compatible AspNetCore services.</para>
    /// <para>Implement <see cref="Setup"/> method to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares (see <see cref="IVostokAspNetCoreApplicationBuilder"/>).</para>
    /// <para>Override <see cref="WarmupAsync"/> method to perform any additional initialization after the DI container gets built but before the app gets registered in service discovery.</para>
    /// </summary>
    [PublicAPI]
    [RequiresPort]
    public abstract class VostokAspNetCoreApplication<TStartup> : IVostokApplication, IDisposable
        where TStartup : class
    {
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        private readonly AtomicBoolean initialized = new AtomicBoolean(false);

        private volatile IHostApplicationLifetime lifetime;
        private volatile IHost host;
        private volatile ILog log;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var builder = new VostokAspNetCoreApplicationBuilder<TStartup>(environment, disposables, initialized);

            // Note(kungurtsev): for code, packed into another dll.
            builder.SetupPingApi(settings => settings.CommitHashProvider = GetCommitHash);

            Setup(builder, environment);

            disposables.Add(host = builder.Build());

            await StartHostAsync(environment).ConfigureAwait(false);

            await WarmupAsync(environment, host.Services).ConfigureAwait(false);

            initialized.TrySetTrue();
        }

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            RunHostAsync();

        /// <summary>
        /// Override this method to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middleware components.
        /// </summary>
        public virtual void Setup([NotNull] IVostokAspNetCoreApplicationBuilder builder, [NotNull] IVostokHostingEnvironment environment)
        {
        }

        /// <summary>
        /// Override this method to perform any initialization that needs to happen after DI container is built and host is started, but before registering in service discovery.
        /// </summary>
        public virtual Task WarmupAsync([NotNull] IVostokHostingEnvironment environment, [NotNull] IServiceProvider serviceProvider) =>
            Task.CompletedTask;

        public void Dispose()
            => disposables.ForEach(disposable => disposable?.Dispose());

        private async Task StartHostAsync(IVostokHostingEnvironment environment)
        {
            log = typeof(TStartup) == typeof(EmptyStartup)
                ? environment.Log.ForContext<VostokAspNetCoreApplication>()
                : environment.Log.ForContext<VostokAspNetCoreApplication<TStartup>>();

            lifetime = (IHostApplicationLifetime)host.Services.GetService(typeof(IHostApplicationLifetime));

            disposables.Add(
                environment.ShutdownToken.Register(
                    () => host
                        .StopAsync()
                        .ContinueWith(t => log.Error(t.Exception, "Failed to stop Host."), TaskContinuationOptions.OnlyOnFaulted)));

            log.Info("Starting Host.");

            await host.StartAsync(environment.ShutdownToken).ConfigureAwait(false);

            await lifetime.ApplicationStarted.WaitAsync().ConfigureAwait(false);

            log.Info("Host started.");
        }

        private async Task RunHostAsync()
        {
            await lifetime.ApplicationStopping.WaitAsync().ConfigureAwait(false);
            log.Info("Stopping Host.");

            await lifetime.ApplicationStopped.WaitAsync().ConfigureAwait(false);
            log.Info("Host stopped.");

            host.Dispose();
        }

        private string GetCommitHash()
        {
            try
            {
                return AssemblyCommitHashExtractor.ExtractFromAssembly(Assembly.GetAssembly(GetType()));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}