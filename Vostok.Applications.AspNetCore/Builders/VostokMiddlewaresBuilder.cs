using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Applications.AspNetCore.StartupFilters;
using Vostok.Commons.Helpers;
using Vostok.Commons.Threading;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokMiddlewaresBuilder
    {
        private readonly VostokThrottlingBuilder throttlingBuilder;

        private readonly Customization<TracingSettings> tracingCustomization = new Customization<TracingSettings>();
        private readonly Customization<LoggingSettings> loggingCustomization = new Customization<LoggingSettings>();
        private readonly Customization<PingApiSettings> pingApiCustomization = new Customization<PingApiSettings>();
        private readonly Customization<UnhandledErrorsSettings> errorHandlingCustomization = new Customization<UnhandledErrorsSettings>();
        private readonly Customization<FillRequestInfoSettings> fillRequestInfoCustomization = new Customization<FillRequestInfoSettings>();
        private readonly Customization<DistributedContextSettings> distributedContextCustomization = new Customization<DistributedContextSettings>();
        private readonly Customization<DatacenterAwarenessSettings> datacenterAwarenessCustomization = new Customization<DatacenterAwarenessSettings>();

        private readonly AtomicBoolean disabled = false;
        private readonly HashSet<Type> disabledMiddlewares = new HashSet<Type>();

        public VostokMiddlewaresBuilder(VostokThrottlingBuilder throttlingBuilder)
            => this.throttlingBuilder = throttlingBuilder;

        public void Disable()
            => disabled.Value = true;

        public void Disable<TMiddleware>()
            => disabledMiddlewares.Add(typeof(TMiddleware));

        public void Customize(Action<TracingSettings> customization)
            => tracingCustomization.AddCustomization(customization);

        public void Customize(Action<LoggingSettings> customization)
            => loggingCustomization.AddCustomization(customization);

        public void Customize(Action<PingApiSettings> customization)
            => pingApiCustomization.AddCustomization(customization);

        public void Customize(Action<UnhandledErrorsSettings> customization)
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

            if (IsEnabled<ThrottlingMiddleware>())
                services.AddSingleton(throttlingBuilder.BuildProvider());

            Register<FillRequestInfoSettings, FillRequestInfoMiddleware>(services, fillRequestInfoCustomization, middlewares);
            Register<DistributedContextSettings, DistributedContextMiddleware>(services, distributedContextCustomization, middlewares);
            Register<TracingSettings, TracingMiddleware>(services, tracingCustomization, middlewares);
            Register<ThrottlingSettings, ThrottlingMiddleware>(services, throttlingBuilder.MiddlewareCustomization, middlewares);
            Register<LoggingSettings, LoggingMiddleware>(services, loggingCustomization, middlewares);
            Register<DatacenterAwarenessSettings, DatacenterAwarenessMiddleware > (services, datacenterAwarenessCustomization, middlewares);
            Register<UnhandledErrorsSettings, UnhandledErrorMiddleware> (services, errorHandlingCustomization, middlewares);
            Register<PingApiSettings, PingApiMiddleware> (services, pingApiCustomization, middlewares);

            if (middlewares.Count == 0)
                return;

            services.AddTransient<IStartupFilter>(_ => new AddMiddlewaresStartupFilter(middlewares));
        }

        private void Register<TSettings, TMiddleware>(IServiceCollection services, Customization<TSettings> customization, List<Type> middlewares)
            where TSettings : class
        {
            if (IsEnabled<TMiddleware>())
            {
                services.Configure<TSettings>(settings => customization.Customize(settings));

                middlewares.Add(typeof(TMiddleware));
            }
        }

        private bool IsEnabled<TMiddleware>()
            => !disabled && !disabledMiddlewares.Contains(typeof(TMiddleware));
    }
}
