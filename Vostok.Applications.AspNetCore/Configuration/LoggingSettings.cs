using System;
using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public class LoggingSettings
    {
        private LoggingCollectionSettings logQueryString = false;
        private LoggingCollectionSettings logRequestHeaders = false;
        private LoggingCollectionSettings logResponseHeaders = false;

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