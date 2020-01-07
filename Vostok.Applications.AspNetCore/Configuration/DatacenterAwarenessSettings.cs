using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public class DatacenterAwarenessSettings
    {
        /// <summary>
        /// <para>If set to <c>true</c>, the application will reject all incoming requests with <see cref="RejectionResponseCode"/> when its local datacenter is not active.</para>
        /// <para>Only enable this if your application is deployed in multiple datacenters.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        public bool RejectRequestsWhenDatacenterIsInactive { get; set; }

        /// <summary>
        /// Rejection response code to be used when <see cref="RejectRequestsWhenDatacenterIsInactive"/> option is enabled.
        /// </summary>
        public int RejectionResponseCode { get; set; } = 503;
    }
}