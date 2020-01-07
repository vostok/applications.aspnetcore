using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Hosting.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore
{
    /// <summary>
    /// <para>Represents a configuration of <see cref="VostokAspNetCoreApplication{TStartup}"/> builder which must be filled during <see cref="VostokAspNetCoreApplication{TStartup}.Setup"/>.</para>
    /// <para>Allows to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares.</para>
    /// </summary>
    [PublicAPI]
    public interface IVostokAspNetCoreApplicationBuilder
    {
        IVostokAspNetCoreApplicationBuilder SetupWebHost([NotNull] Action<IWebHostBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupLogging([NotNull] Action<LoggingMiddlewareSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupTracing([NotNull] Action<TracingMiddlewareSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupThrottling([NotNull] Action<ThrottlingMiddlewareSettings> setup);

        IVostokAspNetCoreApplicationBuilder DenyRequestsIfNotInActiveDatacenter(int denyResponseCode = (int)Clusterclient.Core.Model.ResponseCode.ServiceUnavailable);

        IVostokAspNetCoreApplicationBuilder SetupPingApi([NotNull] Action<PingApiMiddlewareSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupFillRequestInfo([NotNull] Action<FillRequestInfoMiddlewareSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupRestoreDistributedContext([NotNull] Action<RestoreDistributedContextMiddlewareSettings> setup);
        
        IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}