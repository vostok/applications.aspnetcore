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
        /// <para>Request query parameters logging options</para>
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
        /// <para>Request headers logging options</para>
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
        /// <para>Response headers logging options</para>
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