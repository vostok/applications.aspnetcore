using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture]
    public class PingApiMiddlewareTests : ControllerTests
    {
        [Test]
        public async Task GetPing_ShouldReturnOk_WhenReplicaInitialized()
        {
            var response = await Client.GetAsync<PingApiResponse>("/_status/ping");

            response.Status.Should().Be("Ok");
        }

        [Test]
        public async Task GetPing_ShouldReturnWarn_WhenReplicaIsNotHealthy()
        {
            TestEnvironment.IsHealthy = false;

            var response = await Client.GetAsync<PingApiResponse>("/_status/ping");

            response.Status.Should().Be("Warn");
            TestEnvironment.IsHealthy = true;
        }

        [Test]
        public async Task GetVersion_ShouldReturnCorrectCommitHash_WhenFunctorProvided()
        {
            var response = await Client.GetAsync<PingApiResponse>("/_status/version");

            response.CommitHash.Should().Be(TestEnvironment.CommitHash);
        }
    }
}