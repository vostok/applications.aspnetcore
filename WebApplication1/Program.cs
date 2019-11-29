using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Configuration.Sources.Object;
using Vostok.Context;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.AspNetCore;
using Vostok.Hosting.AspNetCore.Setup;
using Vostok.Hosting.Extensions.Houston;
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
                            .AddLog(new DummyLog())
                            .CustomizeLog(l => l.WithMinimumLevel(LogLevel.Info)))
                    .SetupServiceBeacon(
                        serviceBeaconSetup => serviceBeaconSetup
                            .SetupReplicaInfo(
                                replicaInfoSetup => replicaInfoSetup
                                    .SetPort(5050)
                                    .SetApplication("vostok-aspnetcore-test")
                                    ))
                    .SetupConfiguration(
                        configurationSetup => configurationSetup
                            .AddSource(new ObjectSource(new MySettings {SomeString = "some string value"})))
                    .SetupHerculesSink(x => x.EnableVerboseLogging())
                    //.SetupHostExtensions(SetupHostExtensions);
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
            public override void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
            {
                builder
                    .SetupWebHost(
                        webHostSetup => webHostSetup
                            .UseStartup<Startup>()
                            //.UseUrls($"http://*:42222/")
                    )
                    .SetupLoggingMiddleware(
                        logSetup => logSetup
                            .CustomizeSettings(
                                middlewareSettings =>
                                {
                                    middlewareSettings.LogQueryString.WhitelistKeys = new []{"b"};
                                }))
                    .SetupMicrosoftLog(
                        mSetup => mSetup
                            .SetActionLogScopeEnabled(false))
                    .SetupDenyRequestsMiddleware(
                        denyRequests => denyRequests
                            .DenyRequestsIfNotInActiveDatacenter())
                    .SetupPingApiMiddleware(
                        pingApiBuilder => pingApiBuilder.SetCommitHashProvider(() => "hello"))
                    .SetupTracingMiddleware(
                        tracing => tracing
                            .CustomizeSettings(tracingSettings => tracingSettings.ResponseTraceIdHeader = "TraceId"));
            }

            public override async Task WarmupAsync(IVostokHostingEnvironment environment)
            {
                var log = environment.Log.ForContext("Warmup");

                var client = new ClusterClient(
                    log,
                    setup =>
                    {
                        setup.SetupUniversalTransport();
                        environment.ClusterClientSetup(setup);
                        setup.ClusterProvider = new FixedClusterProvider($"{environment.ServiceBeacon.ReplicaInfo.Replica}");
                    });

                var values = await client.SendAsync(Request.Get("api/values?a=a123&b=bb&c=ccc")).ConfigureAwait(false);
                log.Info("Recieved values: {Values}.", values.Response.Content.ToString());
            }
        }

        internal class MyState
        {
            
        }

        internal class MySettings
        {
            public string SomeString { get; set; }
        }
    }
}
