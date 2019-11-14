using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public interface IVostokAspNetCoreApplicationBuilder
    {
        IVostokAspNetCoreApplicationBuilder SetupWebHost([NotNull] Action<IWebHostBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupLoggingMiddleware([NotNull] Action<IVostokLoggingMiddlewareBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupTracingMiddleware([NotNull] Action<IVostokTracingMiddlewareBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupDenyRequestsMiddleware([NotNull] Action<IVostokDenyRequestsMiddlewareBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupPingApiMiddleware([NotNull] Action<IVostokPingApiMiddlewareBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<IVostokMicrosoftLogBuilder> setup);
    }
}