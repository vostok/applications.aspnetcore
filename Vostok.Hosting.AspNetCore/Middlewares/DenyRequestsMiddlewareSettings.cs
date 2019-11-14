using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Configuration of the <see cref="DenyRequestsMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class DenyRequestsMiddlewareSettings
    {
        public DenyRequestsMiddlewareSettings([NotNull] Func<bool> enabled)
        {
            Enabled = enabled;
        }

        /// <summary>
        /// A delegate that decides whether or not to deny all incoming requests.
        /// </summary>
        [NotNull]
        public Func<bool> Enabled { get; }

        /// <summary>
        /// Response code, that will be returned for denied requests.
        /// </summary>
        public int ResponseCode { get; set; } = (int)Clusterclient.Core.Model.ResponseCode.Gone;
    }
}