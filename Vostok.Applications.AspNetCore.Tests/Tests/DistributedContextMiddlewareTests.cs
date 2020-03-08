using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    public class DistributedContextMiddlewareTests : ControllerTests
    {
        [Theory]
        public async Task Invoke_ShouldRestoreRequestPriority(RequestPriority requestPriority)
        {
            var req = Request.Get("/context?name=request-priority");
            var priority = await Client.SendAsync(req, RequestParameters.Empty.WithPriority(requestPriority))
                .GetResponseOrDie<RequestPriority>();

            priority.Should().Be(requestPriority);
        }

        [Test]
        public async Task Invoke_ShouldSetOrdinaryPriority_IfNonePassed()
        {
            var priority = await Client.GetAsync<RequestPriority>("/context?name=request-priority");

            priority.Should().Be(RequestPriority.Ordinary);
        }

        [Test]
        public async Task Invoke_ShouldExecuteAdditionalActions()
        {
            var customContextual = await Client.GetAsync<string>("/context?name=custom-contextual&custom-contextual=some-value");

            customContextual.Should().Be("some-value");
        }
    }
}