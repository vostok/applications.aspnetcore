using System;
using JetBrains.Annotations;
using Vostok.Throttling;
using Vostok.Throttling.Quotas;

namespace Vostok.Applications.AspNetCore.Builders
{
    // CR(kungurtsev): выставлять сразу и ThrottlingSettings.AddXxxProperty.
    [PublicAPI]
    public static class IVostokThrottlingBuilderExtensions
    {
        [NotNull]
        public static IVostokThrottlingBuilder DisableThrottling([NotNull] this IVostokThrottlingBuilder builder)
            => builder.CustomizeSettings(s => s.Enabled = _ => false);

        [NotNull]
        public static IVostokThrottlingBuilder DisableMetrics([NotNull] this IVostokThrottlingBuilder builder)
            => builder.CustomizeSettings(s => s.Metrics = null);

        #region Consumer quotas

        [NotNull]
        public static IVostokThrottlingBuilder UseConsumerQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] PropertyQuotaOptions value)
           => builder.UsePropertyQuota(WellKnownThrottlingProperties.Consumer, value);

        [NotNull]
        public static IVostokThrottlingBuilder UseConsumerQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] Func<PropertyQuotaOptions> provider)
            => builder.UsePropertyQuota(WellKnownThrottlingProperties.Consumer, provider);

        #endregion

        #region Priority quotas

        [NotNull]
        public static IVostokThrottlingBuilder UsePriorityQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] PropertyQuotaOptions value)
            => builder.UsePropertyQuota(WellKnownThrottlingProperties.Priority, value);

        [NotNull]
        public static IVostokThrottlingBuilder UsePriorityQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] Func<PropertyQuotaOptions> provider)
            => builder.UsePropertyQuota(WellKnownThrottlingProperties.Priority, provider);

        #endregion

        #region Method quotas

        [NotNull]
        public static IVostokThrottlingBuilder UseMethodQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] PropertyQuotaOptions value)
            => builder.UsePropertyQuota(WellKnownThrottlingProperties.Method, value);

        [NotNull]
        public static IVostokThrottlingBuilder UseMethodQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] Func<PropertyQuotaOptions> provider)
            => builder.UsePropertyQuota(WellKnownThrottlingProperties.Method, provider);

        #endregion

        #region Url quotas

        [NotNull]
        public static IVostokThrottlingBuilder UseUrlQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] PropertyQuotaOptions value)
            => builder.UsePropertyQuota(WellKnownThrottlingProperties.Url, value);

        [NotNull]
        public static IVostokThrottlingBuilder UseUrlQuota([NotNull] this IVostokThrottlingBuilder builder, [NotNull] Func<PropertyQuotaOptions> provider)
            => builder.UsePropertyQuota(WellKnownThrottlingProperties.Url, provider);

        #endregion
    }
}
