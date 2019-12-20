using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.AspNetCore.Builders;
using Vostok.Logging.Abstractions;
using Vostok.Commons.Helpers.Extensions;

namespace Vostok.Hosting.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreApplication"/> is the abstract class developers inherit from in order to create Vostok-compatible AspNetCore service.</para>
    /// <para>Implement <see cref="Setup"/> method to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares (see <see cref="IVostokAspNetCoreApplicationBuilder"/>).</para>
    /// <para>Override <see cref="WarmupAsync"/> method to perform any additional initialization after the DI container gets built but before the app gets registered in service discovery.</para>
    /// </summary>
    [PublicAPI]
    [RequiresPort]
    public abstract class VostokAspNetCoreApplication : IVostokApplication, IDisposable
    {
        private IHostApplicationLifetime lifetime;
        private ILog log;
        private IHost host;
        private CancellationTokenRegistration shutdownTokenRegistration;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var builder = new AspNetCoreApplicationBuilder();

            Setup(builder, environment);

            host = builder.Build(environment);

            await StartHostAsync(environment).ConfigureAwait(false);

            // CR(iloktionov): А почему warmup вызывается после того, как приложение уже начало слушать порт? Оно может быть ещё не готово к этому.
            // CR(kungurtsev): Чтобы иметь возможность подёргать за контроллеры и прогреть их.
            // CR(kungurtsev): Для всего остального есть Startup.
            await WarmupAsync(environment, host.Services).ConfigureAwait(false);
        }

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            RunHostAsync();

        /// <summary>
        /// Implement this method to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middleware components.
        /// </summary>
        public abstract void Setup([NotNull] IVostokAspNetCoreApplicationBuilder builder, [NotNull] IVostokHostingEnvironment environment);

        /// <summary>
        /// Override this method to perform any initialization that needs to happen after DI container is built, but before registering in service discovery.
        /// </summary>
        public virtual Task WarmupAsync([NotNull] IVostokHostingEnvironment environment, [NotNull] IServiceProvider serviceProvider) =>
            Task.CompletedTask;

        private async Task StartHostAsync(IVostokHostingEnvironment environment)
        {
            log = environment.Log.ForContext<VostokAspNetCoreApplication>();

            lifetime = (IHostApplicationLifetime)host.Services.GetService(typeof(IHostApplicationLifetime));

            shutdownTokenRegistration = environment.ShutdownToken.Register(
                () => host
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop Host."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Starting Host.");

            await host.StartAsync(environment.ShutdownToken).ConfigureAwait(false);

            await lifetime.ApplicationStarted.WaitAsync();
            log.Info("Host started.");
        }

        private async Task RunHostAsync()
        {
            await lifetime.ApplicationStopping.WaitAsync();
            log.Info("Stopping Host.");

            await lifetime.ApplicationStopped.WaitAsync();
            log.Info("Host stopped.");

            host.Dispose();
        }

        public void Dispose()
        {
            shutdownTokenRegistration.Dispose();
            host?.Dispose();
        }
    }
}
