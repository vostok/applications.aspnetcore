#if NET6_0
using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    /// <summary>
    /// <para>Builds the configuration of <see cref="VostokAspNetCoreWebApplication"/>.</para>
    /// <para>Can be customized in app's <see cref="VostokAspNetCoreWebApplication.SetupAsync"/> method.</para>
    /// <para>Allows to configure <see cref="WebApplicationBuilder" />, <see cref="WebApplication" /> and customize built-in Vostok middlewares.</para>
    /// </summary>
    [PublicAPI]
    public interface IVostokAspNetCoreWebApplicationBuilder
    {
        IVostokAspNetCoreWebApplicationBuilder SetupWebApplicationBuilder([NotNull] Action<WebApplicationBuilder> setup);
        
        IVostokAspNetCoreWebApplicationBuilder SetupWebApplication([NotNull] Action<WebApplication> setup);
        
        /// <summary>
        /// Disables web host and HTTP server entirely.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder DisableWebHost();
        
        /// <summary>
        /// Disables all built-in Vostok middlewares. 
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder DisableVostokMiddlewares();

        /// <summary>
        /// <para>Disables the built-in Vostok middleware of given type <typeparamref name="TMiddleware"></typeparamref>.</para>
        /// <para>Current list of Vostok middlewares:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="HttpContextTweakMiddleware"/></description></item>
        ///     <item><description><see cref="FillRequestInfoMiddleware"/></description></item>
        ///     <item><description><see cref="DistributedContextMiddleware"/></description></item>
        ///     <item><description><see cref="TracingMiddleware"/></description></item>
        ///     <item><description><see cref="ThrottlingMiddleware"/></description></item>
        ///     <item><description><see cref="LoggingMiddleware"/></description></item>
        ///     <item><description><see cref="DatacenterAwarenessMiddleware"/></description></item>
        ///     <item><description><see cref="UnhandledExceptionMiddleware"/></description></item>
        ///     <item><description><see cref="PingApiMiddleware"/></description></item>
        ///     <item><description><see cref="DiagnosticApiMiddleware"/></description></item>
        /// </list>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder DisableVostokMiddleware<TMiddleware>();
        
        /// <summary>
        /// <para>Enables the built-in Vostok middleware of given type <typeparamref name="TMiddleware"></typeparamref>.</para>
        /// <para>Use this method to partially enable vostok middlewares after calling <see cref="DisableVostokMiddlewares"/>.</para>
        /// <para>Current list of Vostok middlewares:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="HttpContextTweakMiddleware"/></description></item>
        ///     <item><description><see cref="FillRequestInfoMiddleware"/></description></item>
        ///     <item><description><see cref="DistributedContextMiddleware"/></description></item>
        ///     <item><description><see cref="TracingMiddleware"/></description></item>
        ///     <item><description><see cref="ThrottlingMiddleware"/></description></item>
        ///     <item><description><see cref="LoggingMiddleware"/></description></item>
        ///     <item><description><see cref="DatacenterAwarenessMiddleware"/></description></item>
        ///     <item><description><see cref="UnhandledExceptionMiddleware"/></description></item>
        ///     <item><description><see cref="PingApiMiddleware"/></description></item>
        ///     <item><description><see cref="DiagnosticApiMiddleware"/></description></item>
        /// </list>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder EnableVostokMiddleware<TMiddleware>();

        /// <summary>
        /// Injects a singleton middleware of type <typeparamref name="TMiddleware"/> before Vostok middleware of type <typeparamref name="TBefore"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder InjectPreVostokMiddleware<TMiddleware, TBefore>();

        /// <summary>
        /// Injects a singleton middleware of type <typeparamref name="TMiddleware"/> before the earliest Vostok middleware.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder InjectPreVostokMiddleware<TMiddleware>();

        /// <summary>
        /// Customizes Kestrel settings.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupKestrel([NotNull] Action<KestrelSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="LoggingMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupLogging([NotNull] Action<LoggingSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="TracingMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupTracing([NotNull] Action<TracingSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="ThrottlingMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupThrottling([NotNull] Action<IVostokThrottlingBuilder> setup);

        /// <summary>
        /// Customizes built-in <see cref="DatacenterAwarenessMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupDatacenterAwareness([NotNull] Action<DatacenterAwarenessSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="PingApiMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupPingApi([NotNull] Action<PingApiSettings> setup);

        /// <summary>
        /// Customizes built-in diagnostic features (info providers and health checks).
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupDiagnosticFeatures([NotNull] Action<DiagnosticFeaturesSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="DiagnosticApiMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupDiagnosticApi([NotNull] Action<DiagnosticApiSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="FillRequestInfoMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupRequestInfoFilling([NotNull] Action<FillRequestInfoSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="DistributedContextMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupDistributedContext([NotNull] Action<DistributedContextSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="UnhandledExceptionMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupUnhandledExceptions([NotNull] Action<UnhandledExceptionSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="HttpContextTweakMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupHttpContextTweaks([NotNull] Action<HttpContextTweakSettings> setup);

        /// <summary>
        /// Customizes the adapter between Vostok <see cref="Vostok.Logging.Abstractions.ILog"/> and Microsoft <see cref="Microsoft.Extensions.Logging.ILogger"/>.
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}
#endif