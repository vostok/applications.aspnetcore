using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests
{
    public class PingApiMiddlewareTests : MiddlewareTestsBase
    {
        private bool isHealthy = true;
        private string commitHash;

        public PingApiMiddlewareTests(bool webApplication)
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

#if NET6_0_OR_GREATER
        protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupPingApi(ConfigurePingApi);
        }
#endif
        
#if ASPNTCORE_HOSTING
        protected override void SetupGlobal(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)
        {
            builder.Services.AddVostokPingApi(ConfigurePingApi);
        }
#endif

        private void ConfigurePingApi(PingApiSettings obj)
        {
            obj.HealthCheck = () => isHealthy;
            obj.CommitHashProvider = () => commitHash;
        }
    }
}