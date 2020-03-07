using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Represents configuration of <see cref="LoggingMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class LoggingSettings
    {
        private LoggingCollectionSettings logQueryString = false;
        private LoggingCollectionSettings logRequestHeaders = false;
        private LoggingCollectionSettings logResponseHeaders = false;

        /// <summary>
        /// Determines whether to log incoming requests.
        /// </summary>
        public bool LogRequests { get; set; } = true;

        /// <summary>
        /// Determines whether to log outbound responses.
        /// </summary>
        public bool LogResponses { get; set; } = true;
        
        /// <summary>
        /// Determines whether to log full response completion time.
        /// </summary>
        public bool LogResponseCompletion { get; set; } = true;

        /// <summary>
        /// <para>Request query parameters logging options.</para>
        /// <para>By default, query parameters are not logged at all.</para>
        /// </summary>
        [NotNull]
        public LoggingCollectionSettings LogQueryString
        {
            get =>
                logQueryString;
            set =>
                logQueryString = value.ToCaseInsensitive() ?? throw new ArgumentNullException(nameof(LogQueryString));
        }

        /// <summary>
        /// <para>Request headers logging options.</para>
        /// <para>By default, request headers are not logged at all.</para>
        /// </summary>
        [NotNull]
        public LoggingCollectionSettings LogRequestHeaders
        {
            get =>
                logRequestHeaders;
            set =>
                logRequestHeaders = value.ToCaseInsensitive() ?? throw new ArgumentNullException(nameof(LogRequestHeaders));
        }

        /// <summary>
        /// <para>Response headers logging options.</para>
        /// <para>By default, response headers are not logged at all.</para>
        /// </summary>
        [NotNull]
        public LoggingCollectionSettings LogResponseHeaders
        {
            get =>
                logResponseHeaders;
            set =>
                logResponseHeaders = value.ToCaseInsensitive() ?? throw new ArgumentNullException(nameof(LogResponseHeaders));
        }
    }
}