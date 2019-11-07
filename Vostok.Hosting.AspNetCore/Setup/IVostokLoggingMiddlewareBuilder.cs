using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public interface IVostokLoggingMiddlewareBuilder
    {
        IVostokLoggingMiddlewareBuilder CustomizeSettings([NotNull] Action<LoggingMiddlewareSettings> settingsCustomization);
    }
}