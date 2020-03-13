#if NETCOREAPP3_1
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public static class IVostokAspNetCoreApplicationBuilderExtensionsCore
    {
        public static IVostokAspNetCoreApplicationBuilder AddHostedService<THostedService>([NotNull] this IVostokAspNetCoreApplicationBuilder builder)
            where THostedService : class, IHostedService
            => builder.SetupGenericHost(b => b.ConfigureServices(services => services.AddHostedService<THostedService>()));
    }
}
#endif