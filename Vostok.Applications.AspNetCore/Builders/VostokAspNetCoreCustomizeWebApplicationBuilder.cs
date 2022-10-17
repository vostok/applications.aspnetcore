#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokAspNetCoreCustomizeWebApplicationBuilder : VostokAspNetCoreWebApplicationBuilder
    {
        public VostokAspNetCoreCustomizeWebApplicationBuilder(
            IVostokHostingEnvironment environment,
            IVostokApplication application,
            List<IDisposable> disposables)
            : base(environment, application, disposables)
        {
        }

        public void CustomizeWebApplicationBuilder(WebApplicationBuilder webApplicationBuilder)
        {
            WebApplicationCustomizer.SetupWebApplicationBuilder(ConfigureWebHost());
            WebApplicationCustomizer.CustomizeWebApplicationBuilder(webApplicationBuilder);
        }
    }
}
#endif