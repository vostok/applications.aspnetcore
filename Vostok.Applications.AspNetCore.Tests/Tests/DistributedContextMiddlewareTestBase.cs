using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Clusterclient.Core.Model;
using Vostok.Context;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture]
    public class DistributedContextMiddlewareTestBase : ControllerTestBase
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

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder)
        {
            builder.SetupDistributedContext(s => s.AdditionalActions.AddRange(CreateDistributedContextActions()));
        }

        private static IEnumerable<Action<HttpRequest>> CreateDistributedContextActions()
        {
            yield return r =>
            {
                if (r.Query.TryGetValue("custom-contextual", out var value))
                    FlowingContext.Properties.Set("custom-contextual", value.ToString());
            };
        }
    }
}