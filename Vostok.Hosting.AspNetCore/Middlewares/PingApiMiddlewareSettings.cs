using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    [PublicAPI]
    public class PingApiMiddlewareSettings
    {
        public PingApiMiddlewareSettings([NotNull] Func<string> statusProvider, [NotNull] Func<string> commitHashProvider)
        {
            StatusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
            CommitHashProvider = commitHashProvider ?? throw new ArgumentNullException(nameof(commitHashProvider));
        }

        [NotNull]
        public Func<string> StatusProvider { get; }

        [NotNull]
        public Func<string> CommitHashProvider { get; }
    }
}