#if NET6_0
using Microsoft.AspNetCore.Builder;

// ReSharper disable UnusedTypeParameter

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal partial class VostokWebHostBuilder<TStartup>
        where TStartup : class
    {
        public void ConfigureWebHost(WebApplicationBuilder builder)
        {
            if (webHostEnabled)
            {
                RegisterBasePath(builder.Services);

                CreateHealthCheckBuilder().Register(builder.Services);

                middlewaresBuilder.Register(builder.Services);

                ConfigureWebHostInternal(builder.WebHost);
            }
        }
    }
}
#endif