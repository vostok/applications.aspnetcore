using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture]
    public class PingApiMiddlewareTestBase : ControllerTestBase
    {
        private bool isHealthy = true;
        private string commitHash;

        [SetUp]
        public void Setup()
        {
            isHealthy = true;
            commitHash = Guid.NewGuid().ToString();
        }

        [Test]
        public async Task GetPing_ShouldReturnOk_WhenReplicaInitialized()
        {
            var response = await Client.GetAsync<PingApiResponse>("/_status/ping");

            response.Status.Should().Be("Ok");
        }

        [Test]
        public async Task GetPing_ShouldReturnWarn_WhenReplicaIsNotHealthy()
        {
            isHealthy = false;

            var response = await Client.GetAsync<PingApiResponse>("/_status/ping");

            response.Status.Should().Be("Warn");
            isHealthy = true;
        }

        [Test]
        public async Task GetVersion_ShouldReturnCorrectCommitHash_WhenFunctorProvided()
        {
            var response = await Client.GetAsync<PingApiResponse>("/_status/version");

            response.CommitHash.Should().Be(commitHash);
        }

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder)
        {
            void ConfigurePingApi(PingApiSettings obj)
            {
                obj.HealthCheck = () => isHealthy;
                obj.CommitHashProvider = () => commitHash;
            }

            builder.SetupPingApi(ConfigurePingApi);
        }
    }
}