using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Configuration of the <see cref="LoggingMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class LoggingMiddlewareSettings
    {
        private LoggingCollectionMiddlewareSettings logQueryString = false;
        private LoggingCollectionMiddlewareSettings logRequestHeaders = false;
        private LoggingCollectionMiddlewareSettings logResponseHeaders = false;

        public LoggingMiddlewareSettings([NotNull] ILog log)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }

        // CR(iloktionov): Зачем это здесь торчит в паблик?
        /// <summary>
        /// <see cref="ILog"/> that will be used for requests and responses logging.
        /// </summary>
        [NotNull]
        public ILog Log { get; }

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