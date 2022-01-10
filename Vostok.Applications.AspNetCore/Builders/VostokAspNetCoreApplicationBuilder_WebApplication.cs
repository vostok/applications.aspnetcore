#if NET6_0
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal partial class VostokAspNetCoreApplicationBuilder
    {
        public IVostokAspNetCoreApplicationBuilder SetupWebApplicationBuilder(Action<WebApplicationBuilder> setup)
            => Setup(() => hostFactory.SetupHost(setup));

        public IVostokAspNetCoreApplicationBuilder SetupWebApplication(Action<WebApplication> setup)
            => Setup(() => hostFactory.SetupApplication(setup));

        public IVostokAspNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
            => Setup(() => hostFactory.SetupHost(h => setup?.Invoke(h.Host)));

        public IVostokAspNetCoreApplicationBuilder DisableWebHost()
            => Setup(webHostBuilder.Disable);
    }
}
#endif