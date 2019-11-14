using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Configuration of the <see cref="LoggingMiddlewareSettings.LogQueryString"/>, <see cref="LoggingMiddlewareSettings.LogRequestHeaders"/> and <see cref="LoggingMiddlewareSettings.LogResponseHeaders"/>.
    /// </summary>
    [PublicAPI]
    public class LoggingCollectionMiddlewareSettings
    {
        public LoggingCollectionMiddlewareSettings([NotNull] Func<HttpRequest, bool> enabled) =>
            Enabled = enabled ?? throw new ArgumentNullException(nameof(enabled));

        /// <summary>
        /// A delegate that decides whether or not to log parameters of given <see cref="HttpRequest"/>.
        /// </summary>
        [NotNull]
        public Func<HttpRequest, bool> Enabled { get; }

        /// <summary>
        /// <para>Whitelist of parameter keys to be logged.</para>
        /// <para><c>null</c> value allows all keys.</para>
        /// </summary>
        [CanBeNull]
        public IReadOnlyCollection<string> WhitelistKeys { get; set; }
        
        public static implicit operator LoggingCollectionMiddlewareSettings(bool enabled) =>
            new LoggingCollectionMiddlewareSettings(_ => enabled);
    }
}