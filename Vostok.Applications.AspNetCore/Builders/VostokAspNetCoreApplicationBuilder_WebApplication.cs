#if NET6_0
using System;
using Microsoft.AspNetCore.Builder;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal partial class VostokAspNetCoreApplicationBuilder<TStartup>
        where TStartup : class
    {
        public IVostokAspNetCoreApplicationBuilder SetupWebApplicationBuilder(Action<WebApplicationBuilder> setup)
            => Setup(() => hostFactory.SetupBuilder(setup));
    }
}
#endif