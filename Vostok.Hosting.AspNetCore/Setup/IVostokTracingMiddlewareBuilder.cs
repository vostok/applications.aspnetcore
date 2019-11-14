using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// Represents a configuration of <see cref="TracingMiddleware"/> builder.
    /// </summary>
    [PublicAPI]
    public interface IVostokTracingMiddlewareBuilder
    {
        /// <summary>
        /// Delegate which configures <see cref="TracingMiddlewareSettings"/>.
        /// </summary>
        IVostokTracingMiddlewareBuilder CustomizeSettings([NotNull] Action<TracingMiddlewareSettings> settingsCustomization);
    }
}