using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// <para>Represents a configuration of <see cref="VostokAspNetCoreApplication"/> builder which must be filled during <see cref="VostokAspNetCoreApplication.Setup"/>.</para>
    /// <para>It is required to setup <see cref="IWebHostBuilder"/> with custom <see cref="IStartup"/> class.</para>
    /// <para>Doing the following:</para>
    /// <list type="bullet">
    ///     <item><description>Configures url and url path from <see cref="IServiceBeacon"/>.</description></item>
    ///     <item><description>Registers Vostok <see cref="ILog"/> as Microsoft <see cref="ILogger"/>.</description></item>
    ///     <item><description>Registers Vostok <see cref="Configuration.Abstractions.IConfigurationSource"/> as Microsoft <see cref="Microsoft.Extensions.Configuration.IConfigurationSource"/>.</description></item>
    ///     <item><description>Registers all the fields from <see cref="IVostokHostingEnvironment"/> to <see cref="IServiceCollection"/>.</description></item>
    ///     <item><description>Adds <see cref="FillRequestInfoMiddleware"/>.</description></item>
    ///     <item><description>Adds <see cref="RestoreDistributedContextMiddleware"/>.</description></item>
    ///     <item><description>Adds <see cref="TracingMiddleware"/>.</description></item>
    ///     <item><description>Adds <see cref="LoggingMiddleware"/>.</description></item>
    ///     <item><description>Adds <see cref="DenyRequestsMiddleware"/> (if configured).</description></item>
    ///     <item><description>Adds <see cref="PingApiMiddleware"/>.</description></item>
    ///     <item><description>Applies user given <see cref="IWebHostBuilder"/> configuration.</description></item>
    /// </list>
    /// </summary>
    [PublicAPI]
    public interface IVostokAspNetCoreApplicationBuilder
    {
        /// <summary>
        /// Delegate which configures <see cref="IWebHostBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupWebHost([NotNull] Action<IWebHostBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokLoggingMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupLoggingMiddleware([NotNull] Action<IVostokLoggingMiddlewareBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokTracingMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupTracingMiddleware([NotNull] Action<IVostokTracingMiddlewareBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokDenyRequestsMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupDenyRequestsMiddleware([NotNull] Action<IVostokDenyRequestsMiddlewareBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokPingApiMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupPingApiMiddleware([NotNull] Action<IVostokPingApiMiddlewareBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokMicrosoftLogBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<IVostokMicrosoftLogBuilder> setup);
    }
}