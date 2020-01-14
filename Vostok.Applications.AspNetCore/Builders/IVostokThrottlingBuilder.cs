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

        // CR(kungurtsev): в какой момент будут биндится настройки, что тут даётся только провайдер? да и весь Environment доступен в месте использования.
        [NotNull]
        IVostokThrottlingBuilder UseEssentials([NotNull] Func<IConfigurationProvider, ThrottlingEssentials> provider);

        // CR(kungurtsev): не понятно, что за проперти такие, в рамках обработки запроса.
        // CR(kungurtsev): тут явно не хватает доки.
        // CR(kungurtsev): не сразу понимаешь, что есть проперти, которые (name-value), и есть квоты к ним. Давай уберём везде слово value, где это не value проперти.
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
