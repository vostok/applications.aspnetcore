using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Helpers;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Hosting;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class ControllerTestBase
    {
        private VostokHost testHost;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Log = new SynchronousConsoleLog();
            var serverPort = HostUtils.GetFreePort();
            Client = CreateClusterClient(serverPort);

            testHost = await StartHost(serverPort);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            testHost.ShutdownTokenSource.Cancel();
        }

        protected IClusterClient Client { get; private set; }
        protected ILog Log { get; private set; }

        protected virtual void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder)
        {
            // use this method to override host configuration in each test fixture
        }

        private async Task<VostokHost> StartHost(int port)
        {
            var app = new TestVostokAspNetCoreApplication(SetupGlobal);
            var hostSettings = new VostokHostSettings(app, b => SetupEnvironment(b, port));
            var host = new VostokHost(hostSettings);

            var runTask = Task.Run(host.RunAsync);
            var initializationTask = HostUtils.WaitUntilInitialized(Client);

            var completedTask = await Task.WhenAny(runTask, initializationTask);
            if (completedTask == runTask)
                runTask.Result.EnsureSuccess();

            if (completedTask == initializationTask)
                return initializationTask.Result ? host : throw new TimeoutException("Host didn't start");

            runTask.Result.EnsureSuccess();
            throw new InvalidOperationException("Host exit before test execution");
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