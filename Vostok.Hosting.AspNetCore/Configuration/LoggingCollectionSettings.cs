using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of the <see cref="LoggingSettings.LogQueryString"/>, <see cref="LoggingSettings.LogRequestHeaders"/> and <see cref="LoggingSettings.LogResponseHeaders"/>.
    /// </summary>
    [PublicAPI]
    public class LoggingCollectionSettings
    {
        public LoggingCollectionSettings([NotNull] Func<HttpRequest, bool> enabled) =>
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

        /// <summary>
        /// <para>Blacklist of parameter keys to be logged.</para>
        /// <para>Will be applied, if <see cref="WhitelistKeys"/> is <c>null</c>.</para>
        /// <para><c>null</c> value allows all keys.</para>
        /// </summary>
        [CanBeNull]
        public IReadOnlyCollection<string> BlacklistKeys { get; set; }

        public static implicit operator LoggingCollectionSettings(bool enabled) =>
            new LoggingCollectionSettings(_ => enabled);
    }
}