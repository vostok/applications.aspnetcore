using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Hosting;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Applications.AspNetCore.Tests
{
    [SetUpFixture]
    public class TestEnvironment
    {
        public static IClusterClient Client;
        public static ILog Log;

        [OneTimeSetUp]
        public void SetUp()
        {
            Log = new SynchronousConsoleLog();
            var serverPort = GetFreePort();
            StartHost(serverPort);
            Client = CreateClusterClient(serverPort);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
        }

        private static void StartHost(int serverPort)
        {
            var app = new TestVostokAspNetCoreApplication();
            var hostSettings = new VostokHostSettings(app, b => SetupEnvironment(b, serverPort));
            var host = new VostokHost(hostSettings);
            host.RunAsync();
            WaitHostInitialization();
        }

        private static void SetupEnvironment(IVostokHostingEnvironmentBuilder b, int port)
        {
            b.SetupApplicationIdentity(
                    s => s.SetProject("Project")
                        .SetSubproject("SubProject")
                        .SetEnvironment("Env")
                        .SetApplication("App")
                        .SetInstance("Instance"))
                .SetPort(port)
                .SetupLog(s => s.AddLog(Log));

            b.DisableClusterConfig();
        }

        private static IClusterClient CreateClusterClient(int port)
        {
            return new ClusterClient(
                Log,
                s =>
                {
                    s.ClusterProvider = new FixedClusterProvider($"http://localhost:{port}");
                    s.SetupUniversalTransport();
                });
        }

        private static void WaitHostInitialization()
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    var pingApiResponse = Client.GetAsync<PingApiResponse>("/_status/ping").GetAwaiter().GetResult();
                    if (pingApiResponse.Status == "Ok")
                        return;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
        }

        private static int GetFreePort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }
    }
}