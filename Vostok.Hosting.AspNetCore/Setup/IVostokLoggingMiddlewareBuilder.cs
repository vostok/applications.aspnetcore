using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// Represents a configuration of <see cref="LoggingMiddleware"/> builder.
    /// </summary>
    [PublicAPI]
    public interface IVostokLoggingMiddlewareBuilder
    {
        /// <summary>
        /// Delegate which configures <see cref="LoggingMiddlewareSettings"/>.
        /// </summary>
        IVostokLoggingMiddlewareBuilder CustomizeSettings([NotNull] Action<LoggingMiddlewareSettings> settingsCustomization);
    }
}