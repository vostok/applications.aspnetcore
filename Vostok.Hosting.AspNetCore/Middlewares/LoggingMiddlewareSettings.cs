using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    [PublicAPI]
    public class LoggingMiddlewareSettings
    {
        private LoggingCollectionMiddlewareSettings logQueryString = false;
        private LoggingCollectionMiddlewareSettings logRequestHeaders = false;
        private LoggingCollectionMiddlewareSettings logResponseHeaders = false;

        /// <summary>
        /// <para>Whether or not to log query parameters.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        [NotNull]
        public LoggingCollectionMiddlewareSettings LogQueryString
        {
            get =>
                logQueryString;
            set =>
                logQueryString = value ?? throw new ArgumentNullException(nameof(LogQueryString));
        }

        /// <summary>
        /// <para>Whether or not to request headers.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        [NotNull]
        public LoggingCollectionMiddlewareSettings LogRequestHeaders
        {
            get =>
                logRequestHeaders;
            set =>
                logRequestHeaders = value ?? throw new ArgumentNullException(nameof(LogQueryString));
        }

        /// <summary>
        /// <para>Whether or not to log response headers.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        [NotNull]
        public LoggingCollectionMiddlewareSettings LogResponseHeaders
        {
            get =>
                logResponseHeaders;
            set =>
                logResponseHeaders = value ?? throw new ArgumentNullException(nameof(LogQueryString));
        }
    }
}