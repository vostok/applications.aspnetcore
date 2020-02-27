using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class HostManager : IDisposable
    {
        public readonly IHost Host;
        private readonly ILog log;
        private volatile IHostApplicationLifetime lifetime;
        private volatile IDisposable shutdownRegistration;

        public HostManager(IHost host, ILog log)
        {
            this.Host = host;
            this.log = log;
        }

        public async Task StartHostAsync(IVostokHostingEnvironment environment)
        {
            lifetime = (IHostApplicationLifetime)Host.Services.GetService(typeof(IHostApplicationLifetime));

            shutdownRegistration = environment.ShutdownToken.Register(
                () => Host
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop Host."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Starting Host.");

            await Host.StartAsync(environment.ShutdownToken).ConfigureAwait(false);

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