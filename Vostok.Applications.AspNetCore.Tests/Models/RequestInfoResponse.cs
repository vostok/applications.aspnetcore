using System;
using System.Net;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Applications.AspNetCore.Tests.Models
{
    public class RequestInfoResponse
    {
        public TimeSpan Timeout { get; set; }
        public TimeSpan RemainingTimeout { get; set; }
        public RequestPriority Priority { get; set; }
        public string ClientApplicationIdentity { get; set; }
        public IPAddress ClientIpAddress { get; set; }
    }
}