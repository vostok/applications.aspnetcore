#if NET6_0
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public static class IVostokAspNetCoreApplicationBuilderExtensionsCore
    {
        public static IVostokAspNetCoreApplicationBuilder AddHostedService<THostedService>([NotNull] this IVostokAspNetCoreApplicationBuilder builder)
            where THostedService : class, IHostedService
            => builder.SetupWebApplicationBuilder(b => b.Services
                .AddHostedService<THostedService>());

        public static IVostokAspNetCoreApplicationBuilder AddHostedServiceFromApplication<TApplication>([NotNull] this IVostokAspNetCoreApplicationBuilder builder)
            where TApplication : class, IVostokApplication
            => builder.SetupWebApplicationBuilder(b => b.Services
                .AddSingleton<TApplication>()
                .AddHostedService<VostokHostedService<TApplication>>());

        public static IVostokAspNetCoreApplicationBuilder AddHostedServiceFromApplication<TApplication>([NotNull] this IVostokAspNetCoreApplicationBuilder builder, [NotNull] TApplication application)
            where TApplication : class, IVostokApplication
            => builder.SetupWebApplicationBuilder(b => b.Services
                .AddSingleton(application)
                .AddHostedService<VostokHostedService<TApplication>>());
    }
}
#endif