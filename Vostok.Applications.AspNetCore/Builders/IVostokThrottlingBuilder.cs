using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public interface IVostokThrottlingBuilder
    {
        [NotNull]
        IVostokThrottlingBuilder UseEssentials([NotNull] Func<ThrottlingEssentials> essentialsProvider);

        // CR(kungurtsev): не понятно, что за проперти такие, в рамках обработки запроса.
        // CR(kungurtsev): тут явно не хватает доки.
        // CR(kungurtsev): не сразу понимаешь, что есть проперти, которые (name-value), и есть квоты к ним.
        [NotNull]
        IVostokThrottlingBuilder UsePropertyQuota([NotNull] string propertyName, [NotNull] Func<PropertyQuotaOptions> quotaOptionsProvider);

        [NotNull]
        IVostokThrottlingBuilder UseCustomQuota([NotNull] IThrottlingQuota quota);

        [NotNull]
        IVostokThrottlingBuilder CustomizeSettings([NotNull] Action<ThrottlingSettings> customization);
    }
}
