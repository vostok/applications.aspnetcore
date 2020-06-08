using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
#if ASPNETCORE3_1
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
#endif
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.Abstractions;
using HealthCheckResult = Vostok.Hosting.Abstractions.Diagnostics.HealthCheckResult;
using IHealthCheck = Vostok.Hosting.Abstractions.Diagnostics.IHealthCheck;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture]
    public class HealthCheckIntegrationTests : ControllerTestBase
    {
        private volatile IVostokHostingEnvironment environment;

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment env)
        {
            environment = env;

            #if ASPNETCORE3_1
            builder.SetupGenericHost(
                host => host.ConfigureServices(
                    (ctx, services) =>
                    {
                        services.AddHealthChecks().AddCheck("ms", new MicrosoftHealthCheck());
                    }));
            #endif
        }

        [Test]
        public async Task Should_include_vostok_health_checks_in_aspnetcore_middleware()
        {
            #if ASPNETCORE2_1
            return;
            #endif

            await CheckHealthEndpoint(ResponseCode.Ok, "Healthy");

            using (environment.Diagnostics.HealthTracker.RegisterCheck("degraded", new DegradedHealthCheck()))
            {
                await CheckHealthEndpoint(ResponseCode.Ok, "Degraded");

                using (environment.Diagnostics.HealthTracker.RegisterCheck("failing", new FailingHealthCheck()))
                    await CheckHealthEndpoint(ResponseCode.ServiceUnavailable, "Unhealthy");

                await CheckHealthEndpoint(ResponseCode.Ok, "Degraded");
            }

            await CheckHealthEndpoint(ResponseCode.Ok, "Healthy");
        }

        [Test]
        public void Should_include_aspnetcore_health_checks_in_vostok_tracker()
        {
            #if ASPNETCORE2_1
            return;
            #endif

            environment.Diagnostics.HealthTracker.Should().ContainSingle(pair => pair.name == "ms");
        }

        private async Task CheckHealthEndpoint(ResponseCode expectedCode, string expectedStatus)
        {
            var response = (await Client.GetAsync("/health")).Response;

            response.Code.Should().Be(expectedCode);
            response.Content.ToString().Should().Be(expectedStatus);
        }

        private class DegradedHealthCheck : IHealthCheck
        {
            public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken) =>
                Task.FromResult(HealthCheckResult.Degraded("Because I have degraded."));
        }

        private class FailingHealthCheck : IHealthCheck
        {
            public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken) =>
                Task.FromResult(HealthCheckResult.Failing("Because I have failed."));
        }

        #if ASPNETCORE3_1
        private class MicrosoftHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
        {
            public Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken()) =>
                Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
        }
        #endif
    }
}
