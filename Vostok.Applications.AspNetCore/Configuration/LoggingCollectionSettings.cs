using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of the <see cref="LoggingSettings.LogQueryString"/>, <see cref="LoggingSettings.LogRequestHeaders"/> and <see cref="LoggingSettings.LogResponseHeaders"/>.
    /// </summary>
    [PublicAPI]
    public class LoggingCollectionSettings
    {
        public LoggingCollectionSettings([NotNull] Func<HttpRequest, bool> enabled) =>
            Enabled = enabled ?? throw new ArgumentNullException(nameof(enabled));

        public LoggingCollectionSettings(bool enabled)
            : this(_ => enabled)
        {
        }

        /// <summary>
        /// A delegate that decides whether or not to log parameters of given <see cref="HttpRequest"/> or <see cref="HttpResponse"/>.
        /// </summary>
        [NotNull]
        public Func<HttpRequest, bool> Enabled { get; }

        /// <summary>
        /// <para>Whitelist of parameter keys to be logged.</para>
        /// <para><c>null</c> value allows all keys.</para>
        /// <para>Takes precedence over <see cref="BlacklistKeys"/>.</para>
        /// </summary>
        [CanBeNull]
        public IReadOnlyCollection<string> WhitelistKeys { get; set; }

        /// <summary>
        /// <para>Blacklist of parameter keys to be logged.</para>
        /// <para><c>null</c> value allows all keys.</para>
        /// </summary>
        [CanBeNull]
        public IReadOnlyCollection<string> BlacklistKeys { get; set; }

        public static implicit operator LoggingCollectionSettings(bool enabled) =>
            new LoggingCollectionSettings(enabled);
    }
}
