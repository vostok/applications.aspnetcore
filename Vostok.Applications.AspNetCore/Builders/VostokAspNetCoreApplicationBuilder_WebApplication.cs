#if NET6_0
using System;
using Microsoft.AspNetCore.Builder;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal partial class VostokAspNetCoreApplicationBuilder
    {
        public IVostokAspNetCoreApplicationBuilder SetupWebApplicationBuilder(Action<WebApplicationBuilder> setup)
            => Setup(() => hostFactory.SetupBuilder(setup));
        
        public IVostokAspNetCoreApplicationBuilder SetupWebApplication(Action<WebApplication> setup)
            => Setup(() => hostFactory.SetupApplication(setup));
    }
}
#endif