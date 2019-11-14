using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// Represents a configuration of <see cref="FillRequestInfoMiddleware"/> builder.
    /// </summary>
    [PublicAPI]
    public interface IVostokFillRequestInfoMiddlewareBuilder
    {
        /// <summary>
        /// Delegate which configures <see cref="FillRequestInfoMiddlewareSettings"/>.
        /// </summary>
        IVostokFillRequestInfoMiddlewareBuilder CustomizeSettings([NotNull] Action<FillRequestInfoMiddlewareSettings> settingsCustomization);
    }
}