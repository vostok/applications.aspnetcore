#if NETCOREAPP && !NET6_0
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
            => builder.SetupGenericHost(b => b.ConfigureServices(services => services.AddHostedService<THostedService>()));

        public static IVostokAspNetCoreApplicationBuilder AddHostedServiceFromApplication<TApplication>([NotNull] this IVostokAspNetCoreApplicationBuilder builder)
            where TApplication : class, IVostokApplication
            => builder.SetupGenericHost(b => b.ConfigureServices(services =>
            {
                services.AddSingleton<TApplication>();
                services.AddHostedService<VostokHostedService<TApplication>>();
            }));

        public static IVostokAspNetCoreApplicationBuilder AddHostedServiceFromApplication<TApplication>([NotNull] this IVostokAspNetCoreApplicationBuilder builder, [NotNull] TApplication application)
            where TApplication : class, IVostokApplication
            => builder.SetupGenericHost(b => b.ConfigureServices(services =>
            {
                services.AddSingleton(application);
                services.AddHostedService<VostokHostedService<TApplication>>();
            }));
    }
}
#endif