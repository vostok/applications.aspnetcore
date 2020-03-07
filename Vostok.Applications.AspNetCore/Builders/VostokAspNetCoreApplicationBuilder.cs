using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Threading;
using Vostok.Context;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokAspNetCoreApplicationBuilder<TStartup> : IVostokAspNetCoreApplicationBuilder
        where TStartup : class
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly VostokKestrelBuilder kestrelBuilder;
        private readonly VostokThrottlingBuilder throttlingBuilder;
        private readonly VostokMiddlewaresBuilder middlewaresBuilder;
        private readonly VostokWebHostBuilder<TStartup> webHostBuilder;
        private readonly GenericHostFactory genericHostFactory;

        public VostokAspNetCoreApplicationBuilder(IVostokHostingEnvironment environment, List<IDisposable> disposables, AtomicBoolean initialized)
        {
            this.environment = environment;

            genericHostFactory = new GenericHostFactory(environment);
            genericHostFactory.SetupLogger(
                s => s.IgnoredScopes = new HashSet<string>
                {
                    MicrosoftConstants.ActionLogScope,
                    MicrosoftConstants.HostingLogScope,
                    MicrosoftConstants.ConnectionLogScope
                });

            kestrelBuilder = new VostokKestrelBuilder();
            throttlingBuilder = new VostokThrottlingBuilder(environment, disposables);
            middlewaresBuilder = new VostokMiddlewaresBuilder(throttlingBuilder, initialized);
            webHostBuilder = new VostokWebHostBuilder<TStartup>(environment, kestrelBuilder, middlewaresBuilder);
        }

        public IHost BuildHost()
        {
            using (FlowingContext.Globals.Use(environment))
            {
                var hostBuilder = genericHostFactory.CreateHostBuilder();

                webHostBuilder.ConfigureWebHost(hostBuilder);

                return hostBuilder.Build();
            }
        }

        #region SetupComponents

        public IVostokAspNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
            => Setup(() => genericHostFactory.SetupHost(setup));

        public IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
            => Setup(() => genericHostFactory.SetupLogger(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupKestrel(Action<KestrelSettings> setup)
            => Setup(() => kestrelBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupWebHost(Action<IWebHostBuilder> setup)
            => Setup(() => webHostBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder DisableWebHost()
            => Setup(webHostBuilder.Disable);

        public IVostokAspNetCoreApplicationBuilder DisableVostokMiddlewares()
            => Setup(middlewaresBuilder.Disable);

        public IVostokAspNetCoreApplicationBuilder DisableVostokMiddleware<TMiddleware>()
            => Setup(middlewaresBuilder.Disable<TMiddleware>);

        public IVostokAspNetCoreApplicationBuilder SetupDatacenterAwareness(Action<DatacenterAwarenessSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupRequestInfoFilling(Action<FillRequestInfoSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupDistributedContext(Action<DistributedContextSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupTracing(Action<TracingSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupLogging(Action<LoggingSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupPingApi(Action<PingApiSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupUnhandledErrors(Action<UnhandledErrorsSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupThrottling(Action<IVostokThrottlingBuilder> setup)
            => Setup(() => setup(throttlingBuilder));

        private IVostokAspNetCoreApplicationBuilder Setup(Action setup)
        {
            setup();
            return this;
        }

        #endregion
    }
}