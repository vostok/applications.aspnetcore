using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    [PublicAPI]
    public class DenyRequestsMiddlewareSettings
    {
        public DenyRequestsMiddlewareSettings([NotNull] Func<bool> deniedProvider)
        {
            DeniedProvider = deniedProvider;
        }

        /// <summary>
        /// <para>A delegate that checks whether or not to deny all incoming requests.</para>
        /// </summary>
        [NotNull]
        public Func<bool> DeniedProvider { get; }

        /// <summary>
        /// Response code, that will be returned for denied requests.
        /// </summary>
        public int ResponseCode { get; set; } = (int)Clusterclient.Core.Model.ResponseCode.Gone;
    }
}