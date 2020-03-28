using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Vostok.Applications.AspNetCore.Tests.Extensions
{
    internal static class IServiceCollectionExtensions
    {
        public static void OverrideSingleton<TService>(this IServiceCollection services, TService impl)
            where TService : class
        {
            var descriptors = services.Where(s => s.Lifetime == ServiceLifetime.Singleton && s.ServiceType == typeof(TService))
                .ToArray();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddSingleton(impl);
        }
    }
}