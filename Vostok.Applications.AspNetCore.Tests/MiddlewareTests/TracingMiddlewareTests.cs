using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests
{
    public class TracingMiddlewareTests : MiddlewareTestsBase
    {
        private readonly StubSpanSender spanSender = new();

        public TracingMiddlewareTests(bool webApplication)
            : base(webApplication)
        {
            
        }

        [Test]
        public async Task TracingMiddleware_ShouldAddUrlAnnotation_AccordingToServiceBeaconUrl()
        {
            var response = await Client.GetAsync<PingApiResponse>("/_status/ping");

            response.Status.Should().Be("Ok");

            spanSender.CaughtSpans
               .Where(x =>
                    x.Annotations.ContainsKey(WellKnownAnnotations.Http.Request.Url) &&
                    x.Annotations[WellKnownAnnotations.Http.Request.Url].ToString()!.EndsWith("/_status/ping") &&
                    x.Annotations.ContainsKey(WellKnownAnnotations.Http.Client.Name) &&
                    x.Annotations[WellKnownAnnotations.Http.Client.Name].ToString() == "TestClusterClient"
                )
               .Should()
               .NotBeEmpty();
        }

        protected override void SetupGlobal(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupTracer(tracerBuilder => tracerBuilder.AddSpanSender(spanSender));
        }

        private class StubSpanSender : ISpanSender
        {
            public List<ISpan> CaughtSpans = new();

            public void Send(ISpan span) => CaughtSpans.Add(span);
        }
    }
}