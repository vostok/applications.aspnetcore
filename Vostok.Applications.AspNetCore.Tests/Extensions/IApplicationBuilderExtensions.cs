using Vostok.Applications.AspNetCore.Builders;
#pragma warning disable CS0618

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