using System;
using System.Net;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;

namespace Vostok.Hosting.AspNetCore.Models
{
    internal class RequestInfo : IRequestInfo
    {
        public RequestInfo(TimeSpan? requestTimeout, RequestPriority? requestPriority, string clientApplicationIdentity, IPAddress clientIpAddress)
        {
            Timeout = requestTimeout;
            Budget = Timeout.HasValue ? TimeBudget.StartNew(Timeout.Value.Cut(100.Milliseconds(), 0.05)) : null;
            Priority = requestPriority;
            ClientApplicationIdentity = clientApplicationIdentity;
            ClientIpAddress = clientIpAddress;
        }

        public TimeSpan? Timeout { get; }
        
        public TimeSpan? RemainingTimeout => Budget?.Remaining;
        
        public TimeBudget Budget { get; }
        
        public RequestPriority? Priority { get; }
        
        public string ClientApplicationIdentity { get; }
        
        public IPAddress ClientIpAddress { get; }
    }
}