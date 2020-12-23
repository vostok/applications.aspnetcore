#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Logging.Abstractions;

// ReSharper disable MethodSupportsCancellation

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class GenericHostManager : IDisposable
    {
        private readonly IHost host;
        private readonly ILog log;
        private volatile IHostApplicationLifetime lifetime;
        private volatile IDisposable shutdownRegistration;

        public GenericHostManager(IHost host, ILog log)
        {
            this.host = host;
            this.log = log;
        }

        public IServiceProvider Services => host.Services;

        public async Task StartHostAsync(CancellationToken shutdownToken)
        {
            lifetime = (IHostApplicationLifetime)host.Services.GetService(typeof(IHostApplicationLifetime));
            var environment = (IHostEnvironment)host.Services.GetService(typeof(IHostEnvironment));

            shutdownRegistration = shutdownToken.Register(
                () => host
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop generic host."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Generic host is starting..");

            log.Info("Hosting environment: {HostingEnvironment}.", environment.EnvironmentName);

            await host.StartAsync(shutdownToken);

            await lifetime.ApplicationStarted.WaitAsync();

            log.Info("Generic host has started.");
        }

        public async Task RunHostAsync()
        {
            await lifetime.ApplicationStopping.WaitAsync();

            log.Info("Generic host is stopping..");

            await lifetime.ApplicationStopped.WaitAsync();

            log.Info("Generic host has been stopped.");

            host.Dispose();
        }

        public void Dispose()
        {
            host?.Dispose();
            shutdownRegistration?.Dispose();
        }
    }
}
#endif