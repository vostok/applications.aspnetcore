using JetBrains.Annotations;
using Vostok.Datacenters;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of the <see cref="DenyRequestsIfNotInActiveDatacenterMiddleware"/>.
    /// </summary>
    internal class DatacenterAwarenessSettings
    {
        public DatacenterAwarenessSettings([NotNull] IDatacenters datacenters)
        {
            Datacenters = datacenters;
        }

        /// <summary>
        /// <see cref="IDatacenters"/> instance that will be used for checking whether or not local datacenter is active.
        /// </summary>
        [NotNull]
        internal IDatacenters Datacenters { get; }

        /// <summary>
        /// Response code, that will be returned for denied requests.
        /// </summary>
        public int DenyResponseCode { get; set; } = (int)Clusterclient.Core.Model.ResponseCode.ServiceUnavailable;
    }
}