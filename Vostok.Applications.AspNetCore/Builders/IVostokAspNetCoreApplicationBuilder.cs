﻿using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Logging.Microsoft;

// ReSharper disable PartialTypeWithSinglePart

namespace Vostok.Applications.AspNetCore.Builders
{
    /// <summary>
    /// <para>Builds the configuration of <see cref="VostokAspNetCoreApplication{TStartup}"/>.</para>
    /// <para>Can be customized in app's <see cref="VostokAspNetCoreApplication{TStartup}.Setup"/> method.</para>
    /// <para>Allows to configure/disable <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares.</para>
    /// </summary>
    [PublicAPI]
    public partial interface IVostokAspNetCoreApplicationBuilder
    {
        /// <summary>
        /// <para>Forces to configure middleware options with registered customizations even middleware is disabled.</para>
        /// </summary>
        IVostokAspNetCoreApplicationBuilder ForceConfigureMiddlewaresCustomizations();

        /// <summary>
        /// Disables all built-in Vostok middlewares. 
        /// </summary>
        IVostokAspNetCoreApplicationBuilder DisableVostokMiddlewares();

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
        IVostokAspNetCoreApplicationBuilder DisableVostokMiddleware<TMiddleware>();

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
        IVostokAspNetCoreApplicationBuilder EnableVostokMiddleware<TMiddleware>();

        /// <summary>
        /// Injects a singleton middleware of type <typeparamref name="TMiddleware"/> before Vostok middleware of type <typeparamref name="TBefore"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder InjectPreVostokMiddleware<TMiddleware, TBefore>();

        /// <summary>
        /// Injects a singleton middleware of type <typeparamref name="TMiddleware"/> before the earliest Vostok middleware.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder InjectPreVostokMiddleware<TMiddleware>();

        /// <summary>
        /// Applies an arbitrary customization to web host.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupWebHost([NotNull] Action<IWebHostBuilder> setup);

        /// <summary>
        /// Customizes Kestrel settings.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupKestrel([NotNull] Action<KestrelSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="LoggingMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupLogging([NotNull] Action<LoggingSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="TracingMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupTracing([NotNull] Action<TracingSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="ThrottlingMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupThrottling([NotNull] Action<IVostokThrottlingBuilder> setup);

        /// <summary>
        /// Customizes built-in <see cref="DatacenterAwarenessMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupDatacenterAwareness([NotNull] Action<DatacenterAwarenessSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="PingApiMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupPingApi([NotNull] Action<PingApiSettings> setup);

        /// <summary>
        /// Customizes built-in diagnostic features (info providers and health checks).
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupDiagnosticFeatures([NotNull] Action<DiagnosticFeaturesSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="DiagnosticApiMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupDiagnosticApi([NotNull] Action<DiagnosticApiSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="FillRequestInfoMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupRequestInfoFilling([NotNull] Action<FillRequestInfoSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="DistributedContextMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupDistributedContext([NotNull] Action<DistributedContextSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="UnhandledExceptionMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupUnhandledExceptions([NotNull] Action<UnhandledExceptionSettings> setup);

        /// <summary>
        /// Customizes built-in <see cref="HttpContextTweakMiddleware"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupHttpContextTweaks([NotNull] Action<HttpContextTweakSettings> setup);

        /// <summary>
        /// Customizes the adapter between Vostok <see cref="Vostok.Logging.Abstractions.ILog"/> and Microsoft <see cref="Microsoft.Extensions.Logging.ILogger"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}