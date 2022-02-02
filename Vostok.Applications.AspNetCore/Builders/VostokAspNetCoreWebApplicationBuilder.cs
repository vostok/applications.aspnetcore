#if NET6_0
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Applications.AspNetCore.HostBuilders;
using Vostok.Context;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

// ReSharper disable PartialTypeWithSinglePart

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokAspNetCoreWebApplicationBuilder : IVostokAspNetCoreWebApplicationBuilder
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly VostokKestrelBuilder kestrelBuilder;
        private readonly VostokThrottlingBuilder throttlingBuilder;
        private readonly VostokMiddlewaresBuilder middlewaresBuilder;
        private readonly VostokWebHostBuilder webHostBuilder;
        private readonly WebApplicationFactory webApplicationFactory;

        public VostokAspNetCoreWebApplicationBuilder(IVostokHostingEnvironment environment, IVostokApplication application, List<IDisposable> disposables)
        {
            this.environment = environment;

            webApplicationFactory = new WebApplicationFactory(environment, application);
            webApplicationFactory.SetupLogger(s => { s.IgnoredScopePrefixes = new[] {"Microsoft"}; });

            kestrelBuilder = new VostokKestrelBuilder();
            throttlingBuilder = new VostokThrottlingBuilder(environment, disposables);
            middlewaresBuilder = new VostokMiddlewaresBuilder(environment, disposables, throttlingBuilder);
            webHostBuilder = new VostokWebHostBuilder(environment, kestrelBuilder, middlewaresBuilder, disposables, null);
        }

        public WebApplication Build()
        {
            webApplicationFactory.SetupWebApplicationBuilder(b => webHostBuilder.ConfigureWebHost(b));

            lock (FlowingContextSync.Object)
                using (FlowingContext.Globals.Use(environment))
                    return webApplicationFactory.Create();
        }

        public bool IsMiddlewareEnabled<TMiddleware>() =>
            middlewaresBuilder.IsEnabled<TMiddleware>();

        #region SetupComponents

        public IVostokAspNetCoreWebApplicationBuilder SetupWebApplication(Action<WebApplicationBuilder> setup)
            => Setup(() => webApplicationFactory.SetupWebApplicationBuilder(setup));

        public IVostokAspNetCoreWebApplicationBuilder CustomizeWebApplication(Action<WebApplication> customization)
            => Setup(() => webApplicationFactory.SetupWebApplication(customization));

        public IVostokAspNetCoreWebApplicationBuilder DisableWebHost()
            => Setup(webHostBuilder.Disable);

        public IVostokAspNetCoreWebApplicationBuilder DisableVostokMiddlewares()
            => Setup(middlewaresBuilder.Disable);

        public IVostokAspNetCoreWebApplicationBuilder DisableVostokMiddleware<TMiddleware>()
            => Setup(middlewaresBuilder.Disable<TMiddleware>);

        public IVostokAspNetCoreWebApplicationBuilder EnableVostokMiddleware<TMiddleware>()
            => Setup(middlewaresBuilder.Enable<TMiddleware>);

        public IVostokAspNetCoreWebApplicationBuilder InjectPreVostokMiddleware<TMiddleware, TBefore>()
            => Setup(middlewaresBuilder.InjectPreVostok<TMiddleware, TBefore>);

        public IVostokAspNetCoreWebApplicationBuilder InjectPreVostokMiddleware<TMiddleware>()
            => Setup(middlewaresBuilder.InjectPreVostok<TMiddleware>);

        public IVostokAspNetCoreWebApplicationBuilder SetupDatacenterAwareness(Action<DatacenterAwarenessSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupRequestInfoFilling(Action<FillRequestInfoSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupDistributedContext(Action<DistributedContextSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupTracing(Action<TracingSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupKestrel(Action<KestrelSettings> setup)
            => Setup(() => kestrelBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupLogging(Action<LoggingSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupPingApi(Action<PingApiSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupDiagnosticApi(Action<DiagnosticApiSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupDiagnosticFeatures(Action<DiagnosticFeaturesSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupUnhandledExceptions(Action<UnhandledExceptionSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupHttpContextTweaks(Action<HttpContextTweakSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreWebApplicationBuilder SetupThrottling(Action<IVostokThrottlingBuilder> setup)
            => Setup(() => setup(throttlingBuilder));

        public IVostokAspNetCoreWebApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
            => Setup(() => webApplicationFactory.SetupLogger(setup ?? throw new ArgumentNullException(nameof(setup))));

        private IVostokAspNetCoreWebApplicationBuilder Setup(Action setup)
        {
            setup();
            return this;
        }

        #endregion
    }
}
#endif