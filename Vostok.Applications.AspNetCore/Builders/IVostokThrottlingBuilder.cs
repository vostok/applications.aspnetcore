using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public interface IVostokThrottlingBuilder
    {
        /// <summary>
        /// <para>Configures throttling essentials, such as capacity and queue limits, to be obtained from given <paramref name="essentialsProvider"/>.</para>
        /// <para>See <see cref="ThrottlingEssentials"/> for more info.</para>
        /// </summary>
        [NotNull]
        IVostokThrottlingBuilder UseEssentials([NotNull] Func<ThrottlingEssentials> essentialsProvider);

        /// <summary>
        /// <para>Adds a quota for the property with given <paramref name="propertyName"/> and options fetched from <paramref name="quotaOptionsProvider"/>.</para>
        /// <para>Property quotas allow to explicitly limit parallelism of requests with some definite property values.</para>
        /// <para>By default, internal middleware adds all <see cref="WellKnownThrottlingProperties"/> except for the url:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="WellKnownThrottlingProperties.Consumer"/></description></item>
        ///     <item><description><see cref="WellKnownThrottlingProperties.Priority"/></description></item>
        ///     <item><description><see cref="WellKnownThrottlingProperties.Method"/></description></item>
        /// </list>
        /// <para>When dealing with these well-known properties, it is recommended to use extensions over this method instead:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="IVostokThrottlingBuilderExtensions.UseConsumerQuota"/></description></item>
        ///     <item><description><see cref="IVostokThrottlingBuilderExtensions.UsePriorityQuota"/></description></item>
        ///     <item><description><see cref="IVostokThrottlingBuilderExtensions.UseMethodQuota"/></description></item>
        ///     <item><description><see cref="IVostokThrottlingBuilderExtensions.UseUrlQuota"/></description></item>
        /// </list>
        /// </summary>
        [NotNull]
        IVostokThrottlingBuilder UsePropertyQuota([NotNull] string propertyName, [NotNull] Func<PropertyQuotaOptions> quotaOptionsProvider);

        /// <summary>
        /// <para>Adds an instance of custom user-implemented quota.</para>
        /// <para>See <see cref="IThrottlingQuota"/> for more info.</para>
        /// </summary>
        [NotNull]
        IVostokThrottlingBuilder UseCustomQuota([NotNull] IThrottlingQuota quota);

        /// <summary>
        /// Allows to customize advanced settings pertinent to internal middleware responsible for request throttling.
        /// </summary>
        [NotNull]
        IVostokThrottlingBuilder CustomizeSettings([NotNull] Action<ThrottlingSettings> customization);
    }
}
