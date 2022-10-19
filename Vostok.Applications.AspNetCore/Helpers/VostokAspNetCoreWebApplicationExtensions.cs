#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    [PublicAPI]
    public static class VostokAspNetCoreWebApplicationExtensions
    {
        public static void SetupWebApplicationBuilder(
            [NotNull] this WebApplicationBuilder webApplicationBuilder,
            VostokAspNetCoreWebApplicationSetup setup,
            IVostokHostingEnvironment environment,
            out List<IDisposable> disposables
        )
        {
            var initializationCheckService = new InitializationCheckLifeTimeService();
            webApplicationBuilder.Services.AddHostedService(_ => initializationCheckService);

            disposables = new List<IDisposable>();
            var vostokBuilder = new VostokAspNetCoreCustomizeWebApplicationBuilder(environment, new EmptyApplication(), disposables);
            vostokBuilder.SetupPingApi(PingApiSettingsSetup.Get(environment, typeof(WebApplicationBuilder), initializationCheckService.Initialized));

            setup(vostokBuilder, environment);

            vostokBuilder.CustomizeWebApplicationBuilder(webApplicationBuilder);
        }
    }

    internal class EmptyApplication : VostokAspNetCoreWebApplication
    {
        public override Task SetupAsync(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment) =>
            Task.CompletedTask;
    }

    internal class InitializationCheckLifeTimeService : IHostedService
    {
        public AtomicBoolean Initialized { get; } = new AtomicBoolean(false);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Initialized.TrySet(true);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
#endif