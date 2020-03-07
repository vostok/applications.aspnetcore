using System;
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
        private readonly Customization<FillRequestInfoSettings> fillRequestInfoCustomization = new Customization<FillRequestInfoSettings>();
        private readonly Customization<DistributedContextSettings> distributedContextCustomization = new Customization<DistributedContextSettings>();
        private readonly Customization<DatacenterAwarenessSettings> datacenterAwarenessCustomization = new Customization<DatacenterAwarenessSettings>();

        public VostokMiddlewaresBuilder(VostokThrottlingBuilder throttlingBuilder, AtomicBoolean applicationInitialized)
        {
            this.throttlingBuilder = throttlingBuilder;

            Customize(pingApi => pingApi.InitializationCheck = () => applicationInitialized);
        }

        public void Customize(Action<TracingSettings> customization)
            => tracingCustomization.AddCustomization(customization);

        public void Customize(Action<LoggingSettings> customization)
            => loggingCustomization.AddCustomization(customization);

        public void Customize(Action<PingApiSettings> customization)
            => pingApiCustomization.AddCustomization(customization);

        public void Customize(Action<FillRequestInfoSettings> customization)
            => fillRequestInfoCustomization.AddCustomization(customization);

        public void Customize(Action<DistributedContextSettings> customization)
            => distributedContextCustomization.AddCustomization(customization);

        public void Customize(Action<DatacenterAwarenessSettings> customization)
            => datacenterAwarenessCustomization.AddCustomization(customization);

        public void Register(IServiceCollection services)
        {
            services.AddSingleton(throttlingBuilder.BuildProvider());

            services.Configure<TracingSettings>(settings => tracingCustomization.Customize(settings));
            services.Configure<LoggingSettings>(settings => loggingCustomization.Customize(settings));
            services.Configure<PingApiSettings>(settings => pingApiCustomization.Customize(settings));
            services.Configure<ThrottlingSettings>(settings => throttlingBuilder.MiddlewareCustomization.Customize(settings));
            services.Configure<FillRequestInfoSettings>(settings => fillRequestInfoCustomization.Customize(settings));
            services.Configure<DistributedContextSettings>(settings => distributedContextCustomization.Customize(settings));
            services.Configure<DatacenterAwarenessSettings>(settings => datacenterAwarenessCustomization.Customize(settings));

            var addMiddlewaresFilter = new AddMiddlewaresStartupFilter(
                typeof(FillRequestInfoMiddleware),
                typeof(DistributedContextMiddleware),
                typeof(TracingMiddleware),
                typeof(ThrottlingMiddleware),
                typeof(LoggingMiddleware),
                typeof(DatacenterAwarenessMiddleware),
                typeof(UnhandledErrorMiddleware),
                typeof(PingApiMiddleware));

            services.AddTransient<IStartupFilter>(_ => addMiddlewaresFilter);
        }
    }
}
