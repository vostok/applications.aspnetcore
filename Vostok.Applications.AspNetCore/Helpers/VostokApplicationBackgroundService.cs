using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class VostokApplicationBackgroundService<TApplication> : BackgroundService
        where TApplication : IVostokApplication
    {
        private readonly TApplication application;
        private readonly IVostokHostingEnvironment environment;

        public VostokApplicationBackgroundService(TApplication application, IVostokHostingEnvironment environment)
        {
            this.application = application;
            this.environment = environment;
        }

        public override void Dispose()
            => (application as IDisposable)?.Dispose();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var executionEnvironment = environment.WithReplacedShutdownToken(stoppingToken);

            await application.InitializeAsync(executionEnvironment);

            stoppingToken.ThrowIfCancellationRequested();

            await application.RunAsync(executionEnvironment);
        }
    }
}