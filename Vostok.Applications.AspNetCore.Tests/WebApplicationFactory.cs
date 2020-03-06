using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vostok.Hosting.Abstractions;
using Vostok.Metrics;
using Vostok.ServiceDiscovery.Abstractions.Models;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class WebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            var env = CreateHostingEnvironment();
            return new TestHostBuilder(env);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var testBuilder = (TestHostBuilder)builder;
            return testBuilder.Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder hostBuilder)
        {
            hostBuilder
                .UseContentRoot(".")
                .ConfigureLogging(logging => logging.AddNUnit());
        }

        private static IVostokHostingEnvironment CreateHostingEnvironment()
        {
            var env = Substitute.For<IVostokHostingEnvironment>();

            env.Metrics.Instance.Returns(new DevNullMetricContext());
            env.ServiceBeacon.ReplicaInfo.Returns(new ReplicaInfo("tests", "test-app", "http://localhost:5000/"));

            return env;
        }
    }
}