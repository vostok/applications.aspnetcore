using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// Represents a configuration of <see cref="LoggingMiddleware"/> builder.
    /// </summary>
    [PublicAPI]
    public interface IVostokRestoreDistributedContextMiddlewareBuilder
    {
        /// <summary>
        /// Delegate which configures <see cref="RestoreDistributedContextMiddlewareSettings"/>.
        /// </summary>
        IVostokRestoreDistributedContextMiddlewareBuilder CustomizeSettings([NotNull] Action<RestoreDistributedContextMiddlewareSettings> settingsCustomization);
    }
}