using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// Represents a configuration of <see cref="DenyRequestsMiddleware"/> builder.
    /// </summary>
    [PublicAPI]
    public interface IVostokDenyRequestsMiddlewareBuilder
    {
        /// <summary>
        /// Delegate which configures <see cref="DenyRequestsMiddlewareSettings"/>.
        /// </summary>
        IVostokDenyRequestsMiddlewareBuilder CustomizeSettings([NotNull] Action<DenyRequestsMiddlewareSettings> settingsCustomization);

        /// <summary>
        /// Allows request processing, even if current datacenter is not active.
        /// </summary>
        IVostokDenyRequestsMiddlewareBuilder AllowRequestsIfNotInActiveDatacenter();

        /// <summary>
        /// Denies request processing, if current datacenter is not active.
        /// </summary>
        IVostokDenyRequestsMiddlewareBuilder DenyRequestsIfNotInActiveDatacenter();
    }
}