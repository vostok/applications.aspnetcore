using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

namespace Vostok.Applications.AspNetCore.Tests
{
    public abstract class ControllerTestBase
    {
        private readonly bool webApplication;
        private VostokHost testHost;

        protected ControllerTestBase(bool webApplication)
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

            var serverPort = FreeTcpPortFinder.GetFreePort();
            
            Client = CreateClusterClient(serverPort);

            testHost = await StartHost(serverPort);
        }

        [OneTimeTearDown]
        public Task OneTimeTearDown()
            => testHost?.StopAsync();

        protected IClusterClient Client { get; private set; }
        protected ILog Log { get; private set; }

        protected virtual void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            // use this method to override host configuration in each test fixture
        }
        
#if NET6_0
        protected virtual void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            // use this method to override host configuration in each test fixture
        }
#endif

        private async Task<VostokHost> StartHost(int port)
        {
            IVostokApplication app = webApplication
#if NET6_0
                ? new TestVostokAspNetCoreWebApplication(SetupGlobal)
#else
                ? throw new Exception("Should not be called")
#endif
                
                : new TestVostokAspNetCoreApplication(SetupGlobal);
            var hostSettings = new VostokHostSettings(app, b => SetupEnvironment(b, port));
            var host = new VostokHost(hostSettings);

            await host.StartAsync();

            return host;
        }

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