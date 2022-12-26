using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class VostokApplicationBackgroundService<TApplication> : BackgroundService
        where TApplication : IVostokApplication
    {
        private readonly TApplication application;
        private readonly IVostokHostingEnvironment environment;
        private readonly ILog log;

        public VostokApplicationBackgroundService(TApplication application, IVostokHostingEnvironment environment)
        {
            this.application = application;
            this.environment = environment;
            
            log = this.environment.Log.ForContext<VostokApplicationBackgroundService<TApplication>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var executionEnvironment = environment.WithReplacedShutdownToken(stoppingToken);

            log.Info("Initializing application.");
            await application.InitializeAsync(executionEnvironment);

            stoppingToken.ThrowIfCancellationRequested();

            log.Info("Running application.");
            await application.RunAsync(executionEnvironment);
        }
    }
}