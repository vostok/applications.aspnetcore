using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests
{
    public class RequestInfoMiddlewareTests : MiddlewareTestsBase
    {
        public RequestInfoMiddlewareTests(bool webApplication)
            : base(webApplication)
        {
        }

        [Test]
        public async Task Invoke_ShouldFillRequestInfo()
        {
            var request = Request.Get("request-info")
                .WithHeader(HeaderNames.ApplicationIdentity, "TestApplication")
                .WithHeader(HeaderNames.RequestPriority, "Critical");

            var response = await Client.SendAsync(request, timeout: TimeSpan.FromSeconds(20))
                .GetResponseOrDie<RequestInfoResponse>();

            response.Priority.Should().Be(RequestPriority.Critical);
            response.Timeout.Should().BeCloseTo(TimeSpan.FromSeconds(20), 1000);
            response.ClientApplicationIdentity.Should().Be("TestApplication");
        }
    }
}