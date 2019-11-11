using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public interface IVostokFillRequestInfoMiddlewareBuilder
    {
        IVostokFillRequestInfoMiddlewareBuilder CustomizeSettings([NotNull] Action<FillRequestInfoMiddlewareSettings> settingsCustomization);
    }
}