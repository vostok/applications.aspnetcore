using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Logging.Abstractions;

// ReSharper disable MethodSupportsCancellation

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class HostManager : IDisposable
    {
        private readonly IHost host;
        private readonly ILog log;
        private volatile IHostApplicationLifetime lifetime;
        private volatile IDisposable shutdownRegistration;

        public HostManager(IHost host, ILog log)
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
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop Host."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Starting Host.");

            await host.StartAsync(shutdownToken).ConfigureAwait(false);

            await lifetime.ApplicationStarted.WaitAsync().ConfigureAwait(false);

            log.ForContext("Microsoft.Hosting.Lifetime").Info("Hosting environment: {HostingEnvironment}.", environment.EnvironmentName);

            log.Info("Host started.");
        }

        public async Task RunHostAsync()
        {
            await lifetime.ApplicationStopping.WaitAsync().ConfigureAwait(false);

            log.Info("Stopping Host.");

            await lifetime.ApplicationStopped.WaitAsync().ConfigureAwait(false);

            log.Info("Host stopped.");

            host.Dispose();
        }

        public void Dispose()
        {
            host?.Dispose();
            shutdownRegistration?.Dispose();
        }
    }
}