#pragma warning disable 618

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Logging.Abstractions;

// ReSharper disable MethodSupportsCancellation

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal class WebHostManager : IDisposable
    {
        private readonly IWebHost host;
        private readonly ILog log;
        private volatile IApplicationLifetime lifetime;
        private volatile IDisposable shutdownRegistration;

        public WebHostManager(IWebHost host, ILog log)
        {
            this.host = host;
            this.log = log;
        }

        public IServiceProvider Services => host.Services;

        public async Task StartHostAsync(CancellationToken shutdownToken, IVostokHostShutdown vostokHostShutdown)
        {
            lifetime = (IApplicationLifetime)Services.GetService(typeof(IApplicationLifetime));

            var environment = (IHostingEnvironment)Services.GetService(typeof(IHostingEnvironment));

            shutdownRegistration = shutdownToken.Register(
                () => host
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop web host."), TaskContinuationOptions.OnlyOnFaulted));

            lifetime.ApplicationStopping.Register(() => vostokHostShutdown?.Initiate());

            log.Info("Web host is starting..");

            log.Info("Hosting environment: {HostingEnvironment}.", environment.EnvironmentName);

            await host.StartAsync(shutdownToken);

            await lifetime.ApplicationStarted.WaitAsync();

            log.Info("Web host has started.");
        }

        public async Task RunHostAsync()
        {
            await lifetime.ApplicationStopping.WaitAsync();

            log.Info("Web host is stopping..");

            await lifetime.ApplicationStopped.WaitAsync();

            log.Info("Web host has been stopped.");

            host.Dispose();
        }

        public void Dispose()
        {
            host?.Dispose();
            shutdownRegistration?.Dispose();
        }
    }
}