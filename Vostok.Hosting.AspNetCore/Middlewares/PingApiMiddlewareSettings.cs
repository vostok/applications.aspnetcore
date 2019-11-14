using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Configuration of the <see cref="PingApiMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class PingApiMiddlewareSettings
    {
        public PingApiMiddlewareSettings([NotNull] Func<string> statusProvider, [NotNull] Func<string> commitHashProvider)
        {
            StatusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
            CommitHashProvider = commitHashProvider ?? throw new ArgumentNullException(nameof(commitHashProvider));
        }

        /// <summary>
        /// A delegate that returns current application status.
        /// </summary>
        [NotNull]
        public Func<string> StatusProvider { get; }

        /// <summary>
        /// A delegate that returns application commit hash.
        /// </summary>
        [NotNull]
        public Func<string> CommitHashProvider { get; }
    }
}