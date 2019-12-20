using System;
using System.Net;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;

namespace Vostok.Hosting.AspNetCore.Models
{
    internal class RequestInfo : IRequestInfo
    {
        private readonly TimeBudget timeBudget;

        public RequestInfo(TimeSpan? requestTimeout, RequestPriority? requestPriority, string clientApplicationIdentity, IPAddress clientIpAddress)
        {
            Timeout = requestTimeout;

            if (requestTimeout.HasValue)
                timeBudget = TimeBudget.StartNew(requestTimeout.Value);

            Priority = requestPriority;
            ClientApplicationIdentity = clientApplicationIdentity;
            ClientIpAddress = clientIpAddress;
        }

        public TimeSpan? Timeout { get; }
        public TimeSpan? RemainingTimeout => timeBudget?.Remaining;
        public RequestPriority? Priority { get; }
        public string ClientApplicationIdentity { get; }
        public IPAddress ClientIpAddress { get; }
    }
}