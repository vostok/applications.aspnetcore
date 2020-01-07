using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore
{
    /// <summary>
    /// <para>Represents a configuration of <see cref="VostokAspNetCoreApplication{TStartup}"/> builder which must be filled during <see cref="VostokAspNetCoreApplication{TStartup}.Setup"/>.</para>
    /// <para>You can configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares.</para>
    /// </summary>
    [PublicAPI]
    public interface IVostokAspNetCoreApplicationBuilder
    {
        /// <summary>
        /// Delegate which configures <see cref="IWebHostBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupWebHost([NotNull] Action<IWebHostBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="LoggingMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupLogging([NotNull] Action<LoggingMiddlewareSettings> setup);

        /// <summary>
        /// Delegate which configures <see cref="TracingMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupTracing([NotNull] Action<TracingMiddlewareSettings> setup);

        /// <summary>
        /// <para>Denies request processing, if local datacenter is not active.</para>
        /// <para>Use this option only if your application hosted in multiple datacenters.</para>
        /// </summary>
        IVostokAspNetCoreApplicationBuilder DenyRequestsIfNotInActiveDatacenter(int denyResponseCode = (int)Clusterclient.Core.Model.ResponseCode.ServiceUnavailable);

        /// <summary>
        /// Delegate which configures <see cref="PingApiMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupPingApi([NotNull] Action<PingApiMiddlewareSettings> setup);

        /// <summary>
        /// Delegate which configures <see cref="FillRequestInfoMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupFillRequestInfo([NotNull] Action<FillRequestInfoMiddlewareSettings> setup);

        /// <summary>
        /// Delegate which configures <see cref="RestoreDistributedContextMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupRestoreDistributedContext([NotNull] Action<RestoreDistributedContextMiddlewareSettings> setup);

        /// <summary>
        /// Delegate which configures <see cref="VostokLoggerProviderSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupThrottling([NotNull] Action<ThrottlingMiddlewareSettings> setup);
    }
}