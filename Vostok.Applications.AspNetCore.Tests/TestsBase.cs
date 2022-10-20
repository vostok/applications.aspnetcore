using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

namespace Vostok.Applications.AspNetCore.Tests
{
    [TestFixture(false)]
#if NET6_0_OR_GREATER
    [TestFixture(true)]
#endif
    public abstract partial class TestsBase
    {
        private int serverPort;
        private ITestHostRunner runner;
        private readonly bool webApplication;

        protected TestsBase()
        {
        }

        protected TestsBase(bool webApplication)
        {
            this.webApplication = webApplication;
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Log = new CompositeLog(
                new SynchronousConsoleLog(),
                new FileLog(new FileLogSettings
                {
                    FileOpenMode = FileOpenMode.Rewrite
                }));

            Client = CreateClusterClient(GetPort());
            CreateRunner(b => SetupEnvironment(b, GetPort()));

            await runner.StartAsync();
        }

        [OneTimeTearDown]
        public Task OneTimeTearDown()
            => runner.StopAsync();

        protected IClusterClient Client { get; private set; }
        protected ILog Log { get; private set; }

        protected virtual void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            // use this method to override host configuration in each test fixture
        }

#if NET6_0_OR_GREATER
        protected virtual void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            // use this method to override host configuration in each test fixture
        }
        
        protected virtual void SetupGlobal(WebApplicationBuilder builder)
        {
            // use this method to override host configuration in each test fixture
        }
        
        protected virtual void SetupGlobal(WebApplication builder)
        {
            // use this method to override host configuration in each test fixture
        }
#endif

        private void SetupEnvironment(IVostokHostingEnvironmentBuilder builder, int port)
        {
            builder.SetupApplicationIdentity(
                    s => s.SetProject("Project")
                        .SetSubproject("SubProject")
                        .SetEnvironment("Env")
                        .SetApplication("App")
                        .SetInstance("Instance"))
                .SetPort(port)
                .SetupLog(s => s.AddLog(Log));

            builder.DisableClusterConfig();

            SetupVostokEnvironment(builder);
        }

        protected virtual void SetupVostokEnvironment(IVostokHostingEnvironmentBuilder builder)
        {
        }

        protected int GetPort()
        {
            return serverPort == 0
                ? serverPort = FreeTcpPortFinder.GetFreePort()
                : serverPort;
        }

        private IClusterClient CreateClusterClient(int port)
        {
            // ReSharper disable once RedundantNameQualifier
            // full type name currently required due to https://github.com/vostok/clusterclient.datacenters/issues/1
            return new Clusterclient.Core.ClusterClient(
                Log,
                s =>
                {
                    s.ClusterProvider = new FixedClusterProvider($"http://localhost:{port}");
                    s.SetupUniversalTransport();
                });
        }
    }
}