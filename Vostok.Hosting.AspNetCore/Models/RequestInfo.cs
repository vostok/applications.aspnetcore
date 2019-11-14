using System;
using System.Net;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hosting.AspNetCore.Models
{
    internal class RequestInfo : IRequestInfo
    {
        public RequestInfo(TimeSpan? requestTimeout, RequestPriority? requestPriority, string clientApplicationIdentity, IPAddress clientIpAddress)
        {
            Timeout = requestTimeout;
            Priority = requestPriority;
            ClientApplicationIdentity = clientApplicationIdentity;
            ClientIpAddress = clientIpAddress;
        }

        public TimeSpan? Timeout { get; }
        public RequestPriority? Priority { get; }
        public string ClientApplicationIdentity { get; }
        public IPAddress ClientIpAddress { get; }
    }
}