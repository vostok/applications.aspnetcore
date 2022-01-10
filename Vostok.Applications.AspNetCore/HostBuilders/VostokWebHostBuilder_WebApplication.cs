#if NET6_0
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Builders;

// ReSharper disable UnusedTypeParameter

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal partial class VostokWebHostBuilder
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
        
        private VostokHealthChecksBuilder CreateHealthCheckBuilder()
            => new VostokHealthChecksBuilder(environment, disposables);
    }
}
#endif