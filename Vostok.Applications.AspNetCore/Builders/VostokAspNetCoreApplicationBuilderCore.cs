#if NETCOREAPP3_1
using System;
using Microsoft.Extensions.Hosting;

// ReSharper disable UnusedTypeParameter

namespace Vostok.Applications.AspNetCore.Builders
{
    internal partial class VostokAspNetCoreApplicationBuilder<TStartup>
        where TStartup : class
    {
        public IVostokAspNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
            => Setup(() => hostFactory.SetupHost(setup));

        public IVostokAspNetCoreApplicationBuilder DisableWebHost()
            => Setup(webHostBuilder.Disable);
    }
}
#endif