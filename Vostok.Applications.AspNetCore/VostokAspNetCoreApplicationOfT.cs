using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Commons.Environment;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Logging.Abstractions;
#if NETCOREAPP3_1
using HostManager = Vostok.Applications.AspNetCore.Helpers.GenericHostManager;
#else
using HostManager = Vostok.Applications.AspNetCore.Helpers.WebHostManager;
#endif

namespace Vostok.Applications.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreApplication{TStartup}" /> is the abstract class developers inherit from in order to create Vostok-compatible AspNetCore services.</para>
    /// <para>Implement <see cref="Setup" /> method to configure <see cref="IWebHostBuilder" /> and customize built-in Vostok middlewares (see
    /// <see
    ///     cref="IVostokAspNetCoreApplicationBuilder" />
    /// ).</para>
    /// <para>Override <see cref="WarmupAsync" /> method to perform any additional initialization after the DI container gets built but before the app gets registered in service discovery.</para>
    /// </summary>
    [PublicAPI]
    [RequiresPort]
    public abstract class VostokAspNetCoreApplication<TStartup> : IVostokApplication, IDisposable
        where TStartup : class
    {
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        private readonly AtomicBoolean initialized = new AtomicBoolean(false);
        private volatile HostManager manager;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var log = environment.Log.ForContext<VostokAspNetCoreApplication>();

            var builder = new VostokAspNetCoreApplicationBuilder<TStartup>(environment, disposables);

            builder.SetupPingApi(
                settings =>
                {
                    settings.CommitHashProvider = GetCommitHash;
                    settings.InitializationCheck = () => initialized;
                    settings.HealthCheck = () => environment.Diagnostics.HealthTracker.CurrentStatus == HealthStatus.Healthy;
                });

            Setup(builder, environment);

            disposables.Add(manager = new HostManager(builder.BuildHost(), log));

            await manager.StartHostAsync(environment.ShutdownToken);

            await WarmupAsync(environment, manager.Services);

            initialized.TrySetTrue();
        }

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            manager.RunHostAsync();

        /// <summary>
        /// Override this method to configure <see cref="IWebHostBuilder" /> and customize built-in Vostok middleware components.
        /// </summary>
        public virtual void Setup([NotNull] IVostokAspNetCoreApplicationBuilder builder, [NotNull] IVostokHostingEnvironment environment)
        {
        }

        /// <summary>
        /// Override this method to perform any initialization that needs to happen after DI container is built and host is started, but before registering in service discovery.
        /// </summary>
        public virtual Task WarmupAsync([NotNull] IVostokHostingEnvironment environment, [NotNull] IServiceProvider serviceProvider) =>
            Task.CompletedTask;

        /// <summary>
        /// Override this method to perform any dispose actions that needs to happen after host has been stopped.
        /// </summary>
        public virtual void DoDispose()
        {
        }

        public void Dispose()
        {
            disposables.ForEach(disposable => disposable?.Dispose());
            DoDispose();
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