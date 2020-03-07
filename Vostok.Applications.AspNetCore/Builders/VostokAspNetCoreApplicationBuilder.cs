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
        private readonly VostokNetCoreApplicationBuilder baseHostBuilder;
        private readonly VostokWebHostBuilder<TStartup> webHostBuilder;

        public VostokAspNetCoreApplicationBuilder(IVostokHostingEnvironment environment, List<IDisposable> disposables, AtomicBoolean initialized)
        {
            this.environment = environment;

            baseHostBuilder = new VostokNetCoreApplicationBuilder(environment);
            baseHostBuilder.SetupMicrosoftLog(
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

        public IHost Build()
        {
            using (FlowingContext.Globals.Use(environment))
            {
                var genericHostBuilder = baseHostBuilder.CreateHostBuilder();

                webHostBuilder.ConfigureWebHost(genericHostBuilder);

                return genericHostBuilder.Build();
            }
        }

        #region SetupComponents

        public IVostokAspNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
        {
            baseHostBuilder.SetupGenericHost(setup);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder DisableWebHost()
        {
            webHostBuilder.Disable();
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupWebHost(Action<IWebHostBuilder> setup)
        {
            webHostBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupKestrel(Action<KestrelSettings> setup)
        {
            kestrelBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupDatacenterAwareness(Action<DatacenterAwarenessSettings> setup)
        {
            middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupRequestInfoFilling(Action<FillRequestInfoSettings> setup)
        {
            middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))); 
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupDistributedContext(Action<DistributedContextSettings> setup)
        {
            middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))); 
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupTracing(Action<TracingSettings> setup)
        {
            middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupLogging(Action<LoggingSettings> setup)
        {
            middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupPingApi(Action<PingApiSettings> setup)
        {
            middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
        {
            baseHostBuilder.SetupMicrosoftLog(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupThrottling(Action<IVostokThrottlingBuilder> setup)
        {
            setup(throttlingBuilder);
            return this;
        }

        #endregion
    }
}