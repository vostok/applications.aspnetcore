using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public static class IVostokAspNetCoreApplicationBuilderExtensions
    {
        public static IVostokAspNetCoreApplicationBuilder AddHostedService<THostedService>([NotNull] this IVostokAspNetCoreApplicationBuilder builder)
            where THostedService : class, IHostedService
            => builder.SetupGenericHost(b => b.ConfigureServices(services => services.AddHostedService<THostedService>()));
    }
}