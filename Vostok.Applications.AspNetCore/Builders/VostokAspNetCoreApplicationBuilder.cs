﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Applications.AspNetCore.HostBuilders;
using Vostok.Context;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;
#if NETCOREAPP
using Host = Microsoft.Extensions.Hosting.IHost;
using HostFactory = Vostok.Applications.AspNetCore.HostBuilders.GenericHostFactory;
#else
using Host = Microsoft.AspNetCore.Hosting.IWebHost;
using HostFactory = Vostok.Applications.AspNetCore.HostBuilders.WebHostFactory;
#endif

// ReSharper disable PartialTypeWithSinglePart

namespace Vostok.Applications.AspNetCore.Builders
{
    internal partial class VostokAspNetCoreApplicationBuilder<TStartup> : IVostokAspNetCoreApplicationBuilder
        where TStartup : class
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly VostokKestrelBuilder kestrelBuilder;
        private readonly VostokThrottlingBuilder throttlingBuilder;
        private readonly VostokMiddlewaresBuilder middlewaresBuilder;
        private readonly VostokWebHostBuilder webHostBuilder;
        private readonly HostFactory hostFactory;

        public VostokAspNetCoreApplicationBuilder(IVostokHostingEnvironment environment, IVostokApplication application, VostokDisposables disposables)
        {
            this.environment = environment;

            hostFactory = new HostFactory(environment, application, disposables);
            hostFactory.SetupLogger(s => s.AddDefaultLoggingSettings());

            kestrelBuilder = new VostokKestrelBuilder();
            throttlingBuilder = new VostokThrottlingBuilder(environment, disposables);
            middlewaresBuilder = new VostokMiddlewaresBuilder(environment, disposables, throttlingBuilder);
            webHostBuilder = new VostokWebHostBuilder(environment, kestrelBuilder, middlewaresBuilder, disposables, typeof(TStartup));
        }

        public Host BuildHost()
        {
            lock (FlowingContextSync.Object)
                using (FlowingContext.Globals.Use(environment))
                {
                    var hostBuilder = hostFactory.CreateHostBuilder();

                    webHostBuilder.ConfigureWebHost(hostBuilder);

                    return hostBuilder.Build();
                }
        }

        public bool IsMiddlewareEnabled<TMiddleware>() =>
            middlewaresBuilder.IsEnabled<TMiddleware>();

        #region SetupComponents

        public IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
            => Setup(() => hostFactory.SetupLogger(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupKestrel(Action<KestrelSettings> setup)
            => Setup(() => kestrelBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupWebHost(Action<IWebHostBuilder> setup)
            => Setup(() => webHostBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder ForceConfigureMiddlewaresCustomizations()
            => Setup(middlewaresBuilder.ForceConfigureMiddlewaresCustomizations);

        public IVostokAspNetCoreApplicationBuilder DisableVostokMiddlewares()
            => Setup(middlewaresBuilder.Disable);

        public IVostokAspNetCoreApplicationBuilder DisableVostokMiddleware<TMiddleware>()
            => Setup(middlewaresBuilder.Disable<TMiddleware>);

        public IVostokAspNetCoreApplicationBuilder EnableVostokMiddleware<TMiddleware>()
            => Setup(middlewaresBuilder.Enable<TMiddleware>);

        public IVostokAspNetCoreApplicationBuilder InjectPreVostokMiddleware<TMiddleware, TBefore>()
            => Setup(middlewaresBuilder.InjectPreVostok<TMiddleware, TBefore>);

        public IVostokAspNetCoreApplicationBuilder InjectPreVostokMiddleware<TMiddleware>()
            => Setup(middlewaresBuilder.InjectPreVostok<TMiddleware>);

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

        public IVostokAspNetCoreApplicationBuilder SetupDiagnosticApi(Action<DiagnosticApiSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupDiagnosticFeatures(Action<DiagnosticFeaturesSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupUnhandledExceptions(Action<UnhandledExceptionSettings> setup)
            => Setup(() => middlewaresBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup))));

        public IVostokAspNetCoreApplicationBuilder SetupHttpContextTweaks(Action<HttpContextTweakSettings> setup)
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