using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public static class IVostokAspNetCoreApplicationBuilderExtensions
    {
        /// <inheritdoc cref="IVostokLoggingMiddlewareBuilder.CustomizeSettings"/>
        public static IVostokAspNetCoreApplicationBuilder CustomizeLoggingMiddlewareSettings(
            this IVostokAspNetCoreApplicationBuilder builder,
            [NotNull] Action<LoggingMiddlewareSettings> settingsCustomization) =>
            builder.SetupLoggingMiddleware(setup => setup.CustomizeSettings(settingsCustomization));

        /// <inheritdoc cref="IVostokTracingMiddlewareBuilder.CustomizeSettings"/>
        public static IVostokAspNetCoreApplicationBuilder CustomizeTracingMiddlewareSettings(
            this IVostokAspNetCoreApplicationBuilder builder,
            [NotNull] Action<TracingMiddlewareSettings> settingsCustomization) =>
            builder.SetupTracingMiddleware(setup => setup.CustomizeSettings(settingsCustomization));
    }
}