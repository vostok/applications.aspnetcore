using System;
using System.Reflection;
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

            var type = application.GetType();
            var a = type.GetCustomAttribute<RequiresPort>(true);

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
                                    //.SetPort(5050)
                                    .SetApplication("vostok-aspnetcore-test")
                                    ))
                    .SetupConfiguration(
                        configurationSetup => configurationSetup
                            .AddSource(new ObjectSource(new MySettings {A = "public A"}))
                            .AddSecretSource(new ObjectSource(new MySettings { B = "secret B" })))
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

        [RequiresConfiguration(typeof(MySettings), "")]
        [RequiresSecretConfiguration(typeof(MySecretSettings), "")]
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
                        setup.ClusterProvider = new FixedClusterProvider($"{environment.ServiceBeacon.ReplicaInfo.Replica}");
                    });

                var values = await client.SendAsync(Request.Get("api/values?a=a123&b=bb&c=ccc")).ConfigureAwait(false);
                log.Info("Recieved values: {Values}.", values.Response.Content.ToString());

                var settings = environment.ConfigurationProvider.Get<MySettings>();
                log.Info($"Settings: {settings.A}/{settings.B}");
                var secretSettings = environment.ConfigurationProvider.Get<MySecretSettings>();
                log.Info($"Secret settings: {secretSettings.A}/{secretSettings.B}");
            }
        }

        internal class MyState
        {
            
        }

        internal class MySettings
        {
            public string A { get; set; }
            public string B { get; set; }
        }

        internal class MySecretSettings
        {
            public string A { get; set; }
            public string B { get; set; }
        }
    }
}
