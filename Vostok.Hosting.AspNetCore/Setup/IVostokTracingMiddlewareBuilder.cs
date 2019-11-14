using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public interface IVostokTracingMiddlewareBuilder
    {
        IVostokTracingMiddlewareBuilder CustomizeSettings([NotNull] Action<TracingMiddlewareSettings> settingsCustomization);
    }
}