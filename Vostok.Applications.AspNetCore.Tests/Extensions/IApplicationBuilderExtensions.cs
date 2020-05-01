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
    }
}