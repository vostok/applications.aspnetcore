using System;
using Microsoft.AspNetCore.Hosting;
using Vostok.Context;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore;
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
                            .AddLog(new EchoLog()))
                    .SetupServiceBeacon(
                        serviceBeaconSetup => serviceBeaconSetup
                            .SetupReplicaInfo(
                                replicaInfoSetup => replicaInfoSetup
                                    .SetPort(5050)
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

        internal class EchoLog : ILog
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
            public override IWebHostBuilder ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder, IVostokHostingEnvironment environment)
            {
                return webHostBuilder.UseStartup<Startup>();
            }
        }
    }
}
