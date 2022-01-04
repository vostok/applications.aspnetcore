#if NETCOREAPP
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Builders;

// ReSharper disable UnusedTypeParameter

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal partial class VostokWebHostBuilder<TStartup>
        where TStartup : class
    {
        public void ConfigureWebHost(IHostBuilder genericHostBuilder)
        {
            if (webHostEnabled)
            {
                genericHostBuilder.ConfigureServices(RegisterBasePath);

                genericHostBuilder.ConfigureServices(CreateHealthCheckBuilder().Register);

                genericHostBuilder.ConfigureServices(middlewaresBuilder.Register);

                genericHostBuilder.ConfigureWebHostDefaults(ConfigureWebHostInternal);
            }
        }

        private VostokHealthChecksBuilder CreateHealthCheckBuilder()
            => new VostokHealthChecksBuilder(environment, disposables);
    }
}
#endif