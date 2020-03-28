using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Datacenters;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class DatacenterMiddlewareTests : ControllerTestBase
    {
        private const int RejectionCode = 503;

        private readonly bool rejectResponses;
        private string[] activeDataCenters = {"local"};

        public DatacenterMiddlewareTests(bool rejectResponses)
        {
            this.rejectResponses = rejectResponses;
        }

        [SetUp]
        public void Setup()
        {
            activeDataCenters = new[] {"local"};
        }

        [Test]
        public async Task Invoke_ShouldNotRejectRequest_WhenOk()
        {
            var result = await Client.GetAsync<PingApiResponse>("_status/ping");

            result.Status.Should().Be("Ok");
        }

        [Test]
        public async Task Invoke_ShouldRejectRequests_WhenBlacklisted_AndOptionIsSet()
        {
            activeDataCenters = Array.Empty<string>();

            var result = await Client.GetAsync("_status/ping");
            var responseCode = (int)result.Response.Code;

            responseCode.Should().Be(rejectResponses ? RejectionCode : 200);
        }

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder)
        {
            void ConfigureDatacenter(DatacenterAwarenessSettings settings)
            {
                settings.RejectRequestsWhenDatacenterIsInactive = rejectResponses;
                settings.RejectionResponseCode = RejectionCode;
            }

            builder.SetupDatacenterAwareness(ConfigureDatacenter);
            builder.OverrideSingleton(CreateDataCentersMock());
        }

        private IDatacenters CreateDataCentersMock()
        {
            var dataCenters = Substitute.For<IDatacenters>();

            dataCenters.GetLocalDatacenter().Returns("local");
            dataCenters.GetActiveDatacenters().Returns(_ => activeDataCenters);

            return dataCenters;
        }
    }
}