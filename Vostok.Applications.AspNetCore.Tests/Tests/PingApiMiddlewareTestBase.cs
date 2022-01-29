using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture(false)]
#if NET6_0
    [TestFixture(true)]
#endif
    public class PingApiMiddlewareTestBase : ControllerTestBase
    {
        private bool isHealthy = true;
        private string commitHash;

        public PingApiMiddlewareTestBase(bool webApplication)
            : base(webApplication)
        {
        }
        
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

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupPingApi(ConfigurePingApi);
        }

#if NET6_0
        protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupPingApi(ConfigurePingApi);
        }
#endif
        
        private void ConfigurePingApi(PingApiSettings obj)
        {
            obj.HealthCheck = () => isHealthy;
            obj.CommitHashProvider = () => commitHash;
        }
    }
}