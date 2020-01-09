using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public class KestrelSettings
    {
        /// <summary>
        /// See <see cref="KestrelServerLimits.MaxRequestBodySize"/>. Note the different default value.
        /// </summary>
        public long? MaxRequestBodySize { get; set; }

        /// <summary>
        /// See <see cref="KestrelServerLimits.MaxRequestLineSize"/>. Note the different default value.
        /// </summary>
        public int MaxRequestLineSize { get; set; } = 32 * 1024;

        /// <summary>
        /// See <see cref="KestrelServerLimits.MaxRequestHeadersTotalSize"/>. Note the different default value.
        /// </summary>
        public int MaxRequestHeadersSize { get; set; } = 64 * 1024;

        /// <summary>
        /// <para>See <see cref="KestrelServerLimits.MaxConcurrentUpgradedConnections"/>. Note the different default value.</para>
        /// <para>Built-in throttling does not affect websocket connections: control their parallelism with this option.</para>
        /// </summary>
        public int? MaxConcurrentWebSocketConnections { get; set; } = 10000;

        /// <summary>
        /// See <see cref="KestrelServerLimits.KeepAliveTimeout"/>.
        /// </summary>
        public TimeSpan? KeepAliveTimeout { get; set; }

        /// <summary>
        /// See <see cref="KestrelServerLimits.RequestHeadersTimeout"/>.
        /// </summary>
        public TimeSpan? RequestHeadersTimeout { get; set; }
    }
}
