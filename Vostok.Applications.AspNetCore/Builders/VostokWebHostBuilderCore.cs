#if NETCOREAPP3_1
using Microsoft.Extensions.Hosting;

// ReSharper disable UnusedTypeParameter

namespace Vostok.Applications.AspNetCore.Builders
{
    internal partial class VostokWebHostBuilder<TStartup>
        where TStartup : class
    {
        public void ConfigureWebHost(IHostBuilder genericHostBuilder)
        {
            if (webHostEnabled)
            {
                genericHostBuilder.ConfigureServices(middlewaresBuilder.Register);

                genericHostBuilder.ConfigureWebHostDefaults(ConfigureWebHostInternal);
            }
        }
    }
}
#endif