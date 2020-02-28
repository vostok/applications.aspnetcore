using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public static class IVostokNetCoreApplicationBuilderExtensions
    {
        public static IVostokNetCoreApplicationBuilder AddHostedService<THostedService>([NotNull] this IVostokNetCoreApplicationBuilder builder)
            where THostedService : class, IHostedService
            => builder.SetupGenericHost(b => b.ConfigureServices(services => services.AddHostedService<THostedService>()));
    }
}