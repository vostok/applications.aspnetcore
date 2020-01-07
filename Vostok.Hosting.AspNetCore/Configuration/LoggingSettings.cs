using System;
using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of the <see cref="LoggingMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class LoggingSettings
    {
        private LoggingCollectionSettings logQueryString = false;
        private LoggingCollectionSettings logRequestHeaders = false;
        private LoggingCollectionSettings logResponseHeaders = false;

        public LoggingSettings([NotNull] ILog log)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// <see cref="ILog"/> instance that will be used for requests and responses logging.
        /// </summary>
        [NotNull]
        internal ILog Log { get; }

        /// <summary>
        /// <para>Whether or not to log query parameters.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        [NotNull]
        public LoggingCollectionSettings LogQueryString
        {
            get =>
                logQueryString;
            set =>
                logQueryString = value ?? throw new ArgumentNullException(nameof(LogQueryString));
        }

        /// <summary>
        /// <para>Whether or not to log request headers.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        [NotNull]
        public LoggingCollectionSettings LogRequestHeaders
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
        public LoggingCollectionSettings LogResponseHeaders
        {
            get =>
                logResponseHeaders;
            set =>
                logResponseHeaders = value ?? throw new ArgumentNullException(nameof(LogQueryString));
        }
    }
}