#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Builder;

// ReSharper disable UnusedTypeParameter

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal partial class VostokWebHostBuilder : IVostokWebHostBuilder
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