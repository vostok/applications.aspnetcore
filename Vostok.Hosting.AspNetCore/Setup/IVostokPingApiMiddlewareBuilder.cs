using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public interface IVostokPingApiMiddlewareBuilder
    {
        IVostokPingApiMiddlewareBuilder Disable();

        IVostokPingApiMiddlewareBuilder SetStatusProvider([NotNull] Func<string> statusProvider);

        IVostokPingApiMiddlewareBuilder SetCommitHashProvider([NotNull] Func<string> commitHashProvider);
    }
}