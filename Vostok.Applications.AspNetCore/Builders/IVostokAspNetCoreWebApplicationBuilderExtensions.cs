#if NET6_0
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public static class IVostokAspNetCoreWebApplicationBuilderExtensions
    {
        public static IVostokAspNetCoreWebApplicationBuilder AddHostedService<THostedService>([NotNull] this IVostokAspNetCoreWebApplicationBuilder builder)
            where THostedService : class, IHostedService
            => builder.SetupWebApplication(b => b.Services.AddHostedService<THostedService>());

        public static IVostokAspNetCoreWebApplicationBuilder AddHostedServiceFromApplication<TApplication>([NotNull] this IVostokAspNetCoreWebApplicationBuilder builder)
            where TApplication : class, IVostokApplication
            => builder.SetupWebApplication(b =>
            {
                b.Services.AddSingleton<TApplication>();
                b.Services.AddHostedService<VostokHostedService<TApplication>>();
            });

        public static IVostokAspNetCoreWebApplicationBuilder AddHostedServiceFromApplication<TApplication>([NotNull] this IVostokAspNetCoreWebApplicationBuilder builder, [NotNull] TApplication application)
            where TApplication : class, IVostokApplication
            => builder.SetupWebApplication(b =>
            {
                b.Services.AddSingleton(application);
                b.Services.AddHostedService<VostokHostedService<TApplication>>();
            });
    }
}
#endif