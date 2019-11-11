using System;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hosting.AspNetCore.Models
{
    internal class RequestInfo : IRequestInfo
    {
        public RequestInfo(TimeSpan? requestTimeout, RequestPriority? requestPriority, string applicationIdentity)
        {
            Timeout = requestTimeout;
            Priority = requestPriority;
            ApplicationIdentity = applicationIdentity;
        }

        public TimeSpan? Timeout { get; }
        public RequestPriority? Priority { get; }
        public string ApplicationIdentity { get; }
    }
}