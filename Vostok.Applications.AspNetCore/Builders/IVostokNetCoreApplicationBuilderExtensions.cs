#if NETCOREAPP
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public static class IVostokNetCoreApplicationBuilderExtensions
    {
        public static IVostokNetCoreApplicationBuilder AddHostedService<THostedService>([NotNull] this IVostokNetCoreApplicationBuilder builder)
            where THostedService : class, IHostedService
            => builder.SetupGenericHost(b => b.ConfigureServices(services => services.AddHostedService<THostedService>()));

        /// <inheritdoc cref="ServiceCollectionExtensions.AddBackgroundServiceFromApplication{TApplication}(Microsoft.Extensions.DependencyInjection.IServiceCollection, TApplication)"/>
        public static IVostokNetCoreApplicationBuilder AddHostedServiceFromApplication<TApplication>([NotNull] this IVostokNetCoreApplicationBuilder builder)
            where TApplication : class, IVostokApplication
            => builder.SetupGenericHost(b => b.ConfigureServices(services =>
            {
                services.AddSingleton<TApplication>();
                services.AddHostedService<VostokApplicationBackgroundService<TApplication>>();
            }));

        /// <inheritdoc cref="ServiceCollectionExtensions.AddBackgroundServiceFromApplication{TApplication}(Microsoft.Extensions.DependencyInjection.IServiceCollection, TApplication)"/>
        public static IVostokNetCoreApplicationBuilder AddHostedServiceFromApplication<TApplication>([NotNull] this IVostokNetCoreApplicationBuilder builder, [NotNull] TApplication application)
            where TApplication : class, IVostokApplication
            => builder.SetupGenericHost(b => b.ConfigureServices(services =>
            {
                services.AddSingleton(_ => application);
                services.AddHostedService<VostokApplicationBackgroundService<TApplication>>();
            }));
    }
}
#endif