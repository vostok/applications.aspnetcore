using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.AspNetCore.Builders;
using Vostok.Hosting.AspNetCore.Setup;
using Vostok.Logging.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Vostok.Hosting.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreApplication"/> is the abstract class developers inherit from in order to create Vostok-compatible AspNetCore service.</para>
    /// <para>Implement <see cref="Setup"/> method to configure <see cref="IWebHostBuilder"/> and customize built-in middlewares (see <see cref="IVostokAspNetCoreApplicationBuilder"/>).</para>
    /// <para>Override <see cref="WarmupAsync"/> method to perform any additional initialization before the app gets registered in service discovery.</para>
    /// </summary>
    [PublicAPI]
    [RequiresPort]
    public abstract class VostokAspNetCoreApplication : IVostokApplication
    {
        private IHostApplicationLifetime lifetime;
        private ILog log;
        private IWebHost webHost;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var builder = new AspNetCoreApplicationBuilder();

            Setup(builder, environment);

            webHost = builder.Build(environment);

            await StartWebHostAsync(environment).ConfigureAwait(false);
            
            await WarmupAsync(environment, webHost.Services).ConfigureAwait(false);
        }

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            RunWebHost();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Setup <see cref="IVostokAspNetCoreApplicationBuilder"/> using given <see cref="IVostokHostingEnvironment"/>.
        /// </summary>
        public abstract void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment);

        /// <summary>
        /// Warmup <see cref="VostokAspNetCoreApplication"/> before <see cref="IVostokHostingEnvironment.ServiceBeacon"/> will be started.
        /// </summary>
        public virtual Task WarmupAsync(IVostokHostingEnvironment environment, IServiceProvider serviceProvider) =>
            Task.CompletedTask;

        private async Task StartWebHostAsync(IVostokHostingEnvironment environment)
        {
            log = environment.Log.ForContext<VostokAspNetCoreApplication>();

            lifetime = (IHostApplicationLifetime)webHost.Services.GetService(typeof(IHostApplicationLifetime));

            environment.ShutdownToken.Register(
                () => webHost
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop WebHost."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Starting WebHost.");
            await webHost.StartAsync().ConfigureAwait(false);
            lifetime.ApplicationStarted.WaitHandle.WaitOne();
            log.Info("WebHost started.");
        }

        private void RunWebHost()
        {
            lifetime.ApplicationStopping.WaitHandle.WaitOne();
            log.Info("Stopping WebHost.");

            lifetime.ApplicationStopped.WaitHandle.WaitOne();
            log.Info("WebHost stopped.");

            webHost.Dispose();
        }
    }
}