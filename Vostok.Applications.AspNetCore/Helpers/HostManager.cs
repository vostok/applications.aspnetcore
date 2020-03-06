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
        internal readonly IHost Host;
        private readonly ILog log;
        private volatile IHostApplicationLifetime lifetime;
        private volatile IDisposable shutdownRegistration;

        public HostManager(IHost host, ILog log)
        {
            Host = host;
            this.log = log;
        }

        public IServiceProvider Services => Host.Services;

        public async Task StartHostAsync(CancellationToken shutdownToken)
        {
            lifetime = (IHostApplicationLifetime)Host.Services.GetService(typeof(IHostApplicationLifetime));
            var environment = (IHostEnvironment)Host.Services.GetService(typeof(IHostEnvironment));

            shutdownRegistration = shutdownToken.Register(
                () => Host
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop Host."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Starting Host.");

            log.Info("Hosting environment: {HostingEnvironment}.", environment.EnvironmentName);

            await Host.StartAsync(shutdownToken).ConfigureAwait(false);

            await lifetime.ApplicationStarted.WaitAsync().ConfigureAwait(false);

            log.Info("Host started.");
        }

        public async Task RunHostAsync()
        {
            await lifetime.ApplicationStopping.WaitAsync().ConfigureAwait(false);

            log.Info("Stopping Host.");

            await lifetime.ApplicationStopped.WaitAsync().ConfigureAwait(false);

            log.Info("Host stopped.");

            Host.Dispose();
        }

        public void Dispose()
        {
            Host?.Dispose();
            shutdownRegistration?.Dispose();
        }
    }
}