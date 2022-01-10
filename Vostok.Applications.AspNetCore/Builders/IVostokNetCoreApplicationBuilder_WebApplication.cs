#if NET6_0

using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public partial interface IVostokNetCoreApplicationBuilder
    {
        [Obsolete("Use `IVostokAspNetCoreApplicationBuilder.SetupWebApplicationBuilder(webApplicationBuilder => webApplicationBuilder.Host...)` instead.")]
        IVostokNetCoreApplicationBuilder SetupGenericHost([NotNull] Action<IHostBuilder> setup);

        IVostokNetCoreApplicationBuilder SetupWebApplicationBuilder([NotNull] Action<WebApplicationBuilder> setup);
        
        IVostokNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}

#endif