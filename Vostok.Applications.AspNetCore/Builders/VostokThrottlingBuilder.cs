using System;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokThrottlingBuilder : IVostokThrottlingBuilder
    {
        private readonly ThrottlingConfigurationBuilder configurationBuilder;
        private readonly Customization<ThrottlingSettings> settingsCustomization;

        public VostokThrottlingBuilder(IVostokHostingEnvironment environment)
        {
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
                    error => environment.Log.Error(error, "Failed to throttle request."));

            settingsCustomization = new Customization<ThrottlingSettings>();
        }

        public (ThrottlingProvider provider, ThrottlingSettings settings) Build()
        {
            var settings = settingsCustomization.Customize(new ThrottlingSettings());
            if (settings.UseThreadPoolOverloadQuota)
                configurationBuilder.AddCustomQuota(new ThreadPoolOverloadQuota(new ThreadPoolOverloadQuotaOptions()));

            return (new ThrottlingProvider(configurationBuilder.Build()), settings);
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

        public IVostokThrottlingBuilder CustomizeSettings(Action<ThrottlingSettings> customization)
        {
            settingsCustomization.AddCustomization(customization);
            return this;
        }
    }
}