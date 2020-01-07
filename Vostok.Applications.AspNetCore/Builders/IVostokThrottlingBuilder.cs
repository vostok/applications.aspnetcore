using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public interface IVostokThrottlingBuilder
    {
        [NotNull]
        IVostokThrottlingBuilder UseEssentials([NotNull] ThrottlingEssentials value);

        [NotNull]
        IVostokThrottlingBuilder UseEssentials([NotNull] Func<ThrottlingEssentials> provider);

        [NotNull]
        IVostokThrottlingBuilder UseEssentials([NotNull] Func<IConfigurationProvider, ThrottlingEssentials> provider);

        [NotNull]
        IVostokThrottlingBuilder UsePropertyQuota([NotNull] string propertyName, [NotNull] PropertyQuotaOptions value);

        [NotNull]
        IVostokThrottlingBuilder UsePropertyQuota([NotNull] string propertyName, [NotNull] Func<PropertyQuotaOptions> provider);

        [NotNull]
        IVostokThrottlingBuilder UsePropertyQuota([NotNull] string propertyName, [NotNull] Func<IConfigurationProvider, PropertyQuotaOptions> provider);

        [NotNull]
        IVostokThrottlingBuilder UseCustomQuota([NotNull] IThrottlingQuota quota);

        [NotNull]
        IVostokThrottlingBuilder CustomizeSettings([NotNull] Action<ThrottlingSettings> customization);
    }
}
