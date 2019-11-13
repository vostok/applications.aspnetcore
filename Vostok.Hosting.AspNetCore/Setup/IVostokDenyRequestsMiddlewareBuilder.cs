using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public interface IVostokDenyRequestsMiddlewareBuilder
    {
        IVostokDenyRequestsMiddlewareBuilder CustomizeSettings([NotNull] Action<DenyRequestsMiddlewareSettings> settingsCustomization);

        IVostokDenyRequestsMiddlewareBuilder AllowRequestsIfNotInActiveDatacenter();

        IVostokDenyRequestsMiddlewareBuilder DenyRequestsIfNotInActiveDatacenter();
    }
}