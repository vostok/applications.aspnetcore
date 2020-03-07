#pragma warning disable 618

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Logging.Abstractions;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

// ReSharper disable MethodSupportsCancellation

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class WebHostManager : IDisposable
    {
        private readonly IWebHost webHost;
        private readonly ILog log;
        private volatile IApplicationLifetime lifetime;
        private volatile IDisposable shutdownRegistration;

        public WebHostManager(IWebHost webHost, ILog log)
        {
            this.webHost = webHost;
            this.log = log;
        }

        public IServiceProvider Services => webHost.Services;

        public async Task StartHostAsync(CancellationToken shutdownToken)
        {
            lifetime = (IApplicationLifetime)Services.GetService(typeof(IApplicationLifetime));

            var environment = (IHostingEnvironment)Services.GetService(typeof(IHostingEnvironment));

            shutdownRegistration = shutdownToken.Register(
                () => webHost
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop WebHost."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Starting WebHost.");

            log.Info("Hosting environment: {HostingEnvironment}.", environment.EnvironmentName);

            await webHost.StartAsync(shutdownToken);

            await lifetime.ApplicationStarted.WaitAsync();

            log.Info("WebHost started.");
        }

        public async Task RunHostAsync()
        {
            await lifetime.ApplicationStopping.WaitAsync();

            log.Info("Stopping WebHost.");

            await lifetime.ApplicationStopped.WaitAsync();

            log.Info("WebHost stopped.");

            webHost.Dispose();
        }

        public void Dispose()
        {
            webHost?.Dispose();
            shutdownRegistration?.Dispose();
        }
    }
}
