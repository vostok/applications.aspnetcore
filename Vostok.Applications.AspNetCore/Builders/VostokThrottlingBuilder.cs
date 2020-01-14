using System;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
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
            configurationBuilder.SetNumberOfCores(
                () =>
                {
                    var limit = environment.ApplicationLimits.CpuUnits;
                    if (limit.HasValue)
                        return (int) Math.Ceiling(limit.Value);

                    return Environment.ProcessorCount;
                });

            settingsCustomization = new Customization<ThrottlingSettings>();
        }

        public (ThrottlingProvider provider, ThrottlingSettings settings) Build()
            => (new ThrottlingProvider(configurationBuilder.Build()), settingsCustomization.Customize(new ThrottlingSettings()));

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
