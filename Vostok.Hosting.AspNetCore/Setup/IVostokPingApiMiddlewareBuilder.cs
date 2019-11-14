using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// Represents a configuration of <see cref="PingApiMiddleware"/> builder.
    /// </summary>
    [PublicAPI]
    public interface IVostokPingApiMiddlewareBuilder
    {
        /// <summary>
        /// Disables <see cref="PingApiMiddleware"/>.
        /// </summary>
        IVostokPingApiMiddlewareBuilder Disable();

        /// <summary>
        /// Sets custom <see cref="PingApiMiddlewareSettings.StatusProvider"/>.
        /// </summary>
        IVostokPingApiMiddlewareBuilder SetStatusProvider([NotNull] Func<string> statusProvider);

        /// <summary>
        /// Sets custom <see cref="PingApiMiddlewareSettings.CommitHashProvider"/>.
        /// </summary>
        IVostokPingApiMiddlewareBuilder SetCommitHashProvider([NotNull] Func<string> commitHashProvider);
    }
}