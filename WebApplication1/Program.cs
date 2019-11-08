using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Context;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore;
using Vostok.Hosting.AspNetCore.Setup;
using Vostok.Hosting.Kontur;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Values;
using Vostok.Logging.Abstractions.Wrappers;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var application = new MyApplication();

            VostokHostingEnvironmentSetup environmentSetup = setup =>
            {
                setup
                    .SetupApplicationIdentity(
                        (applicationIdentitySetup, setupContext) => applicationIdentitySetup
                            .SetProject("Infrastructure")
                            .SetApplication("vostok-aspnetcore-test")
                    )
                    .SetupLog(
                        (logSetup, setupContext) => logSetup
                            .SetupHerculesLog(herculesLogSetup => herculesLogSetup
                                .SetStream("logs_vostoklibs_cloud")
                                .SetApiKeyProvider(() => setupContext.ClusterConfigClient.Get("app/key").Value))
                            .SetupConsoleLog()
                            .AddLog(new DummyLog()))
                    .SetupServiceBeacon(
                        serviceBeaconSetup => serviceBeaconSetup
                            .SetupReplicaInfo(
                                replicaInfoSetup => replicaInfoSetup
                                    .SetPort(5050)
                                    .SetApplication("vostok-aspnetcore-test")
                                    ))
                    ;
            };

            var runner = new VostokHost(new VostokHostSettings(application, environmentSetup).SetupForKontur());

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                runner.ShutdownTokenSource.Cancel();
            };

            var result = runner.RunAsync().GetAwaiter().GetResult();

            Console.WriteLine($"RunResult: {result.State} {result.Error}");
        }

        internal class DummyLog : ILog
        {
            public void Log(LogEvent @event)
            {
                var x = FlowingContext.Globals.Get<OperationContextValue>();
            }

            public bool IsEnabledFor(LogLevel level) =>
                true;

            public ILog ForContext(string context) =>
                new SourceContextWrapper(this, context);
        }

        internal class MyApplication : VostokAspNetCoreApplication
        {
            public override VostokAspNetCoreApplicationSetup Setup(IVostokHostingEnvironment environment) =>
                setup => setup
                    .SetupWebHost(
                        webHostSetup => webHostSetup
                            .UseStartup<Startup>())
                    .SetupLoggingMiddleware(
                        logSetup => logSetup
                            .CustomizeSettings(
                                middlewareSettings =>
                                {
                                    middlewareSettings.LogQueryString = true;
                                    middlewareSettings.LogResponseHeaders = true;
                                }));

            public override async Task WarmUpAsync(IVostokHostingEnvironment environment)
            {
                var log = environment.Log.ForContext("WarmUp");

                var client = new ClusterClient(
                    log,
                    setup =>
                    {
                        setup.SetupUniversalTransport();
                        environment.ClusterClientSetup(setup);
                        setup.ClusterProvider = new FixedClusterProvider($"{environment.ServiceBeacon.ReplicaInfo.Replica}");
                    });

                var values = await client.SendAsync(Request.Get("api/values?a=a123")).ConfigureAwait(false);
                log.Info("Recieved values: {Values}.", values.Response.Content.ToString());
            }
        }
    }
}
