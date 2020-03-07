using System;
using System.Collections.Generic;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokThrottlingBuilder : IVostokThrottlingBuilder
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly List<IDisposable> disposables;
        private readonly ThrottlingConfigurationBuilder configurationBuilder;

        public VostokThrottlingBuilder(IVostokHostingEnvironment environment, List<IDisposable> disposables)
        {
            this.environment = environment;
            this.disposables = disposables;

            configurationBuilder = new ThrottlingConfigurationBuilder();
            configurationBuilder
                .SetNumberOfCores(
                    () =>
                    {
                        var limit = environment.ApplicationLimits.CpuUnits;
                        if (limit.HasValue)
                            return (int)Math.Ceiling(limit.Value);

                        return Environment.ProcessorCount;
                    })
                .SetErrorCallback(
                    error => environment.Log.ForContext<ThrottlingMiddleware>().Error(error, "Internal failure in request throttling."));

            MiddlewareCustomization = new Customization<ThrottlingSettings>();
        }

        public Customization<ThrottlingSettings> MiddlewareCustomization { get; }

        public bool UseThreadPoolOverloadQuota { get; set; } = true;

        public ThrottlingMetricsOptions Metrics { get; set; } = new ThrottlingMetricsOptions();

        public IThrottlingProvider Build()
        {
            if (UseThreadPoolOverloadQuota)
                configurationBuilder.AddCustomQuota(new ThreadPoolOverloadQuota(new ThreadPoolOverloadQuotaOptions()));

            var provider = new ThrottlingProvider(configurationBuilder.Build());

            if (Metrics != null)
                disposables.Add(environment.Metrics.Instance.CreateThrottlingMetrics(provider, Metrics));

            return provider;
        }

        public IVostokThrottlingBuilder UseEssentials(Func<ThrottlingEssentials> essentialsProvider)
        {
            configurationBuilder.SetEssentials(essentialsProvider);
            return this;
        }

        public IVostokThrottlingBuilder UsePropertyQuota(string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider)
        {
            configurationBuilder.SetPropertyQuota(propertyName, quotaOptionsProvider);
            return this;
        }

        public IVostokThrottlingBuilder UseCustomQuota(IThrottlingQuota quota)
        {
            configurationBuilder.AddCustomQuota(quota);
            return this;
        }

        public IVostokThrottlingBuilder CustomizeMiddleware(Action<ThrottlingSettings> customization)
        {
            MiddlewareCustomization.AddCustomization(customization);
            return this;
        }
    }
}