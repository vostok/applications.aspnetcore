#if NET6_0
using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Vostok.Applications.AspNetCore.Builders
{
    public partial interface IVostokAspNetCoreApplicationBuilder
    {
        IVostokAspNetCoreApplicationBuilder SetupWebApplicationBuilder([NotNull] Action<WebApplicationBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupWebApplication([NotNull] Action<WebApplication> setup);
    }
}
#endif