using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    public class TracingMiddlewareTests : TestsBase
    {
        private Uri customUri;
        private StubSpanSender spanSender = new();

        public TracingMiddlewareTests(bool webApplication)
            : base(webApplication)
        {
            customUri = new UriBuilder
            {
                Port = GetPort(),
                Host = "localhost"
            }.Uri;
        }

        [Test]
        public async Task TracingMiddleware_ShouldAddUrlAnnotation_AccordingToServiceBeaconUrl()
        {
            var response = await Client.GetAsync<PingApiResponse>("/_status/ping");

            response.Status.Should().Be("Ok");

            spanSender.CaughtSpans
               .Where(x =>
                    x.Annotations.ContainsKey(WellKnownAnnotations.Http.Request.Url) &&
                    x.Annotations[WellKnownAnnotations.Http.Request.Url].ToString()!.StartsWith(customUri.AbsoluteUri) &&
                    x.Annotations[WellKnownAnnotations.Http.Request.Url].ToString()!.EndsWith("/_status/ping")
                )
               .Should()
               .NotBeEmpty();
        }

        protected override void SetupVostokEnvironment(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupServiceBeacon(beaconBuilder => beaconBuilder.SetupReplicaInfo(infoBuilder => infoBuilder.SetUrl(customUri)));
            builder.SetupTracer(tracerBuilder => tracerBuilder.AddSpanSender(spanSender));
        }

        private class StubSpanSender : ISpanSender
        {
            public List<ISpan> CaughtSpans = new();

            public void Send(ISpan span) => CaughtSpans.Add(span);
        }
    }
}