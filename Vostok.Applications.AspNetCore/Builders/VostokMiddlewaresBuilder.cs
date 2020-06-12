using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Applications.AspNetCore.StartupFilters;
using Vostok.Commons.Helpers;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokMiddlewaresBuilder
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly VostokThrottlingBuilder throttlingBuilder;
        private readonly List<IDisposable> disposables;

        private readonly Customization<TracingSettings> tracingCustomization = new Customization<TracingSettings>();
        private readonly Customization<LoggingSettings> loggingCustomization = new Customization<LoggingSettings>();
        private readonly Customization<PingApiSettings> pingApiCustomization = new Customization<PingApiSettings>();
        private readonly Customization<DiagnosticApiSettings> diagnosticApiCustomization = new Customization<DiagnosticApiSettings>();
        private readonly Customization<DiagnosticFeaturesSettings> diagnosticFeaturesCustomization = new Customization<DiagnosticFeaturesSettings>();
        private readonly Customization<UnhandledExceptionSettings> errorHandlingCustomization = new Customization<UnhandledExceptionSettings>();
        private readonly Customization<FillRequestInfoSettings> fillRequestInfoCustomization = new Customization<FillRequestInfoSettings>();
        private readonly Customization<DistributedContextSettings> distributedContextCustomization = new Customization<DistributedContextSettings>();
        private readonly Customization<DatacenterAwarenessSettings> datacenterAwarenessCustomization = new Customization<DatacenterAwarenessSettings>();

        private readonly AtomicBoolean disabled = false;
        private readonly HashSet<Type> disabledMiddlewares = new HashSet<Type>();
        private readonly Dictionary<Type, List<Type>> preVostokMiddlewares = new Dictionary<Type, List<Type>>();

        public VostokMiddlewaresBuilder(IVostokHostingEnvironment environment, List<IDisposable> disposables, VostokThrottlingBuilder throttlingBuilder)
        {
            this.environment = environment;
            this.disposables = disposables;
            this.throttlingBuilder = throttlingBuilder;
        }

        public void Disable()
            => disabled.Value = true;

        public void Disable<TMiddleware>()
            => disabledMiddlewares.Add(typeof(TMiddleware));

        public void InjectPreVostok<TMiddleware>()
            => InjectPreVostok<TMiddleware, FillRequestInfoMiddleware>();

        public void InjectPreVostok<TMiddleware, TBefore>()
        {
            if (!preVostokMiddlewares.TryGetValue(typeof(TBefore), out var injected))
                preVostokMiddlewares[typeof(TBefore)] = injected = new List<Type>();

            injected.Add(typeof(TMiddleware));
        }

        public void Customize(Action<TracingSettings> customization)
            => tracingCustomization.AddCustomization(customization);

        public void Customize(Action<LoggingSettings> customization)
            => loggingCustomization.AddCustomization(customization);

        public void Customize(Action<PingApiSettings> customization)
            => pingApiCustomization.AddCustomization(customization);

        public void Customize(Action<DiagnosticApiSettings> customization)
            => diagnosticApiCustomization.AddCustomization(customization);

        public void Customize(Action<DiagnosticFeaturesSettings> customization)
            => diagnosticFeaturesCustomization.AddCustomization(customization);

        public void Customize(Action<UnhandledExceptionSettings> customization)
            => errorHandlingCustomization.AddCustomization(customization);

        public void Customize(Action<FillRequestInfoSettings> customization)
            => fillRequestInfoCustomization.AddCustomization(customization);

        public void Customize(Action<DistributedContextSettings> customization)
            => distributedContextCustomization.AddCustomization(customization);

        public void Customize(Action<DatacenterAwarenessSettings> customization)
            => datacenterAwarenessCustomization.AddCustomization(customization);

        public void Register(IServiceCollection services)
        {
            var middlewares = new List<Type>();
            var diagnosticSettings = diagnosticFeaturesCustomization.Customize(new DiagnosticFeaturesSettings());

            RegisterThrottlingProvider(services, diagnosticSettings);
            RegisterRequestTracker(services, diagnosticSettings);

            Register<FillRequestInfoSettings, FillRequestInfoMiddleware>(services, fillRequestInfoCustomization, middlewares);
            Register<DistributedContextSettings, DistributedContextMiddleware>(services, distributedContextCustomization, middlewares);
            Register<TracingSettings, TracingMiddleware>(services, tracingCustomization, middlewares);
            Register<ThrottlingSettings, ThrottlingMiddleware>(services, throttlingBuilder.MiddlewareCustomization, middlewares);
            Register<LoggingSettings, LoggingMiddleware>(services, loggingCustomization, middlewares);
            Register<DatacenterAwarenessSettings, DatacenterAwarenessMiddleware > (services, datacenterAwarenessCustomization, middlewares);
            Register<UnhandledExceptionSettings, UnhandledExceptionMiddleware> (services, errorHandlingCustomization, middlewares);
            Register<PingApiSettings, PingApiMiddleware> (services, pingApiCustomization, middlewares);
            Register<DiagnosticApiSettings, DiagnosticApiMiddleware> (services, diagnosticApiCustomization, middlewares);

            if (middlewares.Count == 0)
                return;

            services.AddTransient<IStartupFilter>(_ => new AddMiddlewaresStartupFilter(middlewares));
        }

        private void Register<TSettings, TMiddleware>(IServiceCollection services, Customization<TSettings> customization, List<Type> middlewares)
            where TSettings : class
        {
            if (preVostokMiddlewares.TryGetValue(typeof(TMiddleware), out var injected))
                middlewares.AddRange(injected);

            if (IsEnabled<TMiddleware>())
            {
                services.Configure<TSettings>(settings => customization.Customize(settings));

                middlewares.Add(typeof(TMiddleware));
            }
        }

        private bool IsEnabled<TMiddleware>()
            => !disabled && !disabledMiddlewares.Contains(typeof(TMiddleware));

        private void RegisterThrottlingProvider(IServiceCollection services, DiagnosticFeaturesSettings settings)
        {
            var throttlingProvider = throttlingBuilder.BuildProvider();

            services.AddSingleton<IThrottlingProvider>(throttlingProvider);

            if (settings.AddThrottlingInfoProvider)
            {
                var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, "request-throttling");
                var infoProvider = new ThrottlingInfoProvider(throttlingProvider);

                disposables.Add(environment.Diagnostics.Info.RegisterProvider(infoEntry, infoProvider));
            }

            if (settings.AddThrottlingHealthCheck)
            {
                var healthCheck = new ThrottlingHealthCheck(throttlingProvider);

                disposables.Add(environment.Diagnostics.HealthTracker.RegisterCheck("Request throttling", healthCheck));
            }
        }

        private void RegisterRequestTracker(IServiceCollection services, DiagnosticFeaturesSettings settings)
        {
            if (settings.AddCurrentRequestsInfoProvider)
            {
                var requestTracker = new RequestTracker();

                services.AddSingleton<IRequestTracker>(requestTracker);

                var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, "requests-in-progress");
                var infoProvider = new CurrentRequestsInfoProvider(requestTracker);

                disposables.Add(environment.Diagnostics.Info.RegisterProvider(infoEntry, infoProvider));
            }
            else services.AddSingleton<IRequestTracker>(new DevNullRequestTracker());
        }
    }
}
