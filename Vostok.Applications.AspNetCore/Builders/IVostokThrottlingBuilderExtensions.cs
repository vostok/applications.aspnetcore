using System;
using JetBrains.Annotations;
using Vostok.Throttling;
using Vostok.Throttling.Quotas;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public static class IVostokThrottlingBuilderExtensions
    {
        [NotNull]
        public static IVostokThrottlingBuilder DisableThrottling([NotNull] this IVostokThrottlingBuilder builder)
            => builder.CustomizeSettings(s => s.Enabled = _ => false);

        [NotNull]
        public static IVostokThrottlingBuilder DisableMetrics([NotNull] this IVostokThrottlingBuilder builder)
            => builder.CustomizeSettings(s => s.Metrics = null);

        [NotNull]
        public static IVostokThrottlingBuilder UseConsumerQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] Func<PropertyQuotaOptions> quotaOptionsProvider) =>
            builder
                .CustomizeSettings(settings => settings.AddConsumerProperty = true)
                .UsePropertyQuota(WellKnownThrottlingProperties.Consumer, quotaOptionsProvider);

        [NotNull]
        public static IVostokThrottlingBuilder UsePriorityQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] Func<PropertyQuotaOptions> quotaOptionsProvider) => 
            builder
                .CustomizeSettings(settings => settings.AddPriorityProperty = true)
                .UsePropertyQuota(WellKnownThrottlingProperties.Priority, quotaOptionsProvider);

        [NotNull]
        public static IVostokThrottlingBuilder UseMethodQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] Func<PropertyQuotaOptions> quotaOptionsProvider) => 
            builder
                .CustomizeSettings(settings => settings.AddMethodProperty = true)
                .UsePropertyQuota(WellKnownThrottlingProperties.Method, quotaOptionsProvider);

        [NotNull]
        public static IVostokThrottlingBuilder UseUrlQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] Func<PropertyQuotaOptions> quotaOptionsProvider) => 
            builder
                .CustomizeSettings(settings => settings.AddUrlProperty = true)
                .UsePropertyQuota(WellKnownThrottlingProperties.Url, quotaOptionsProvider);
    }
}
