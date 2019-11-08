using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Builders;
using Vostok.Hosting.AspNetCore.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore
{
    [PublicAPI]
    public abstract class VostokAspNetCoreApplication : IVostokApplication
    {
        private IApplicationLifetime lifetime;
        private ILog log;
        private IWebHost webHost;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var setup = SetupAspNetCore(environment);

            webHost = AspNetCoreApplicationBuilder.Build(setup, environment);

            await StartWebHostAsync(environment).ConfigureAwait(false);

            await WarmUpAsync(environment).ConfigureAwait(false);
        }

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            RunWebHost();

            return Task.CompletedTask;
        }
        
        public abstract VostokAspNetCoreApplicationSetup SetupAspNetCore(IVostokHostingEnvironment environment);

        public virtual Task WarmUpAsync(IVostokHostingEnvironment environment)
            => Task.CompletedTask;

        private async Task StartWebHostAsync(IVostokHostingEnvironment environment)
        {
            log = environment.Log.ForContext<VostokAspNetCoreApplication>();

            lifetime = (IApplicationLifetime)webHost.Services.GetService(typeof(IApplicationLifetime));

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