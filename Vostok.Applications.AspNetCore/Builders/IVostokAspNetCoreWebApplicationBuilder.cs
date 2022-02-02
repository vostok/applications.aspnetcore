#if NET6_0
using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Configuration;
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
        IVostokAspNetCoreWebApplicationBuilder SetupWebApplication([NotNull] Action<WebApplicationBuilder> setup);

        IVostokAspNetCoreWebApplicationBuilder CustomizeWebApplication([NotNull] Action<WebApplication> customization);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.DisableWebHost"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder DisableWebHost();

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.DisableVostokMiddlewares"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder DisableVostokMiddlewares();

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.DisableVostokMiddleware{TMiddleware}"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder DisableVostokMiddleware<TMiddleware>();

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.EnableVostokMiddleware{TMiddleware}"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder EnableVostokMiddleware<TMiddleware>();

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.InjectPreVostokMiddleware{TMiddleware,TBefore}"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder InjectPreVostokMiddleware<TMiddleware, TBefore>();

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.InjectPreVostokMiddleware{TMiddleware,TBefore}"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder InjectPreVostokMiddleware<TMiddleware>();

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupKestrel"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupKestrel([NotNull] Action<KestrelSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupLogging"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupLogging([NotNull] Action<LoggingSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupTracing"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupTracing([NotNull] Action<TracingSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupThrottling"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupThrottling([NotNull] Action<IVostokThrottlingBuilder> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupDatacenterAwareness"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupDatacenterAwareness([NotNull] Action<DatacenterAwarenessSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupPingApi"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupPingApi([NotNull] Action<PingApiSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupDiagnosticFeatures"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupDiagnosticFeatures([NotNull] Action<DiagnosticFeaturesSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupDiagnosticApi"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupDiagnosticApi([NotNull] Action<DiagnosticApiSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupRequestInfoFilling"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupRequestInfoFilling([NotNull] Action<FillRequestInfoSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupDistributedContext"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupDistributedContext([NotNull] Action<DistributedContextSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupUnhandledExceptions"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupUnhandledExceptions([NotNull] Action<UnhandledExceptionSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupHttpContextTweaks"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupHttpContextTweaks([NotNull] Action<HttpContextTweakSettings> setup);

        /// <summary>
        /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupMicrosoftLog"/>
        /// </summary>
        IVostokAspNetCoreWebApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}
#endif