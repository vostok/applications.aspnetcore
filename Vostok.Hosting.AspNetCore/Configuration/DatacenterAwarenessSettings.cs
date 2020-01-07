using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of the <see cref="DatacenterAwarenessMiddleware"/>.
    /// </summary>
    internal class DatacenterAwarenessSettings
    {
        /// <summary>
        /// Response code, that will be returned for denied requests.
        /// </summary>
        public int DenyResponseCode { get; set; } = (int)Clusterclient.Core.Model.ResponseCode.ServiceUnavailable;
    }
}