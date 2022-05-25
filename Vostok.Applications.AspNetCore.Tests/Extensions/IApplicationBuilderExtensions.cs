using Vostok.Applications.AspNetCore.Builders;

namespace Vostok.Applications.AspNetCore.Tests.Extensions
{
    internal static class IApplicationBuilderExtensions
    {
        public static void OverrideSingleton<TService>(this IVostokAspNetCoreApplicationBuilder builder, TService impl)
            where TService : class
        {
            builder.SetupWebHost(s => s.ConfigureServices((_, c) => c.OverrideSingleton(impl)));
        }

#if NET6_0_OR_GREATER
        public static void OverrideSingleton<TService>(this IVostokAspNetCoreWebApplicationBuilder builder, TService impl)
            where TService : class
        {
            builder.SetupWebApplication(b => b.Services.OverrideSingleton(impl));
        }
#endif
    }
}