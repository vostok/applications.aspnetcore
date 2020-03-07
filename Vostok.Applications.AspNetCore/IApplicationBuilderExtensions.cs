using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore
{
    [PublicAPI]
    public static class IApplicationBuilderExtensions
    {
        #region Distributed context

        /// <summary>
        /// Adds the <see cref="DistributedContextMiddleware"/> to the specified application builder.
        /// </summary>
        public static IApplicationBuilder UseVostokDistributedContext(this IApplicationBuilder builder)
            => builder.UseMiddleware<DistributedContextMiddleware>();

        /// <summary>
        /// Adds the <see cref="DistributedContextMiddleware"/> to the specified application builder.
        /// </summary>
        public static IApplicationBuilder UseVostokDistributedContext(this IApplicationBuilder builder, Action<DistributedContextSettings> setup)
            => builder.UseMiddleware<DistributedContextMiddleware>(Configure(setup));

        #endregion

        #region Datacenter awareness

        /// <summary>
        /// Adds the <see cref="DatacenterAwarenessMiddleware"/> to the specified application builder.
        /// </summary>
        public static IApplicationBuilder UseVostokDatacenterAwareness(this IApplicationBuilder builder)
            => builder.UseMiddleware<DatacenterAwarenessMiddleware>();

        /// <summary>
        /// Adds the <see cref="DatacenterAwarenessMiddleware"/> to the specified application builder.
        /// </summary>
        public static IApplicationBuilder UseVostokDatacenterAwareness(this IApplicationBuilder builder, Action<DatacenterAwarenessSettings> setup)
            => builder.UseMiddleware<DatacenterAwarenessMiddleware>(Configure(setup));

        #endregion

        #region Request info 

        /// <summary>
        /// Adds the <see cref="FillRequestInfoMiddleware"/> to the specified application builder.
        /// </summary>
        public static IApplicationBuilder UseVostokRequestInfo(this IApplicationBuilder builder)
            => builder.UseMiddleware<FillRequestInfoMiddleware>();

        /// <summary>
        /// Adds the <see cref="FillRequestInfoMiddleware"/> to the specified application builder.
        /// </summary>
        public static IApplicationBuilder UseVostokRequestInfo(this IApplicationBuilder builder, Action<FillRequestInfoSettings> setup)
            => builder.UseMiddleware<FillRequestInfoMiddleware>(Configure(setup));

        #endregion

        #region Tracing

        /// <summary>
        /// <para>Adds the <see cref="TracingMiddleware"/> to the specified application builder.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/> and <see cref="DistributedContextMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokTracing(this IApplicationBuilder builder)
            => builder.UseMiddleware<TracingMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="TracingMiddleware"/> to the specified application builder.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/> and <see cref="DistributedContextMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokTracing(this IApplicationBuilder builder, Action<TracingSettings> setup)
            => builder.UseMiddleware<TracingMiddleware>(Configure(setup));

        #endregion

        #region Throttling

        /// <summary>
        /// <para>Adds the <see cref="ThrottlingMiddleware"/> to the specified application builder.</para>
        /// <para>Make sure to register an instance of <see cref="IThrottlingProvider"/> in DI container.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokThrottling(this IApplicationBuilder builder)
            => builder.UseMiddleware<ThrottlingMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="ThrottlingMiddleware"/> to the specified application builder.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokThrottling(this IApplicationBuilder builder, IThrottlingProvider provider)
            => builder.UseMiddleware<ThrottlingMiddleware>(provider);

        /// <summary>
        /// <para>Adds the <see cref="ThrottlingMiddleware"/> to the specified application builder.</para>
        /// <para>Make sure to register an instance of <see cref="IThrottlingProvider"/> in DI container.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokThrottling(this IApplicationBuilder builder, Action<ThrottlingSettings> setup)
            => builder.UseMiddleware<ThrottlingMiddleware>(Configure(setup));

        /// <summary>
        /// <para>Adds the <see cref="ThrottlingMiddleware"/> to the specified application builder.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokThrottling(this IApplicationBuilder builder, IThrottlingProvider provider, Action<ThrottlingSettings> setup)
            => builder.UseMiddleware<ThrottlingMiddleware>(provider, Configure(setup));

        #endregion

        #region Ping API

        /// <summary>
        /// Adds the <see cref="PingApiMiddleware"/> to the specified application builder.
        /// </summary>
        public static IApplicationBuilder UseVostokPingApi(this IApplicationBuilder builder)
            => builder.UseMiddleware<PingApiMiddleware>();

        /// <summary>
        /// Adds the <see cref="PingApiMiddleware"/> to the specified application builder.
        /// </summary>
        public static IApplicationBuilder UseVostokPingApi(this IApplicationBuilder builder, Action<PingApiSettings> setup)
            => builder.UseMiddleware<PingApiMiddleware>(Configure(setup));

        #endregion

        #region Logging

        /// <summary>
        /// <para>Adds the <see cref="LoggingMiddleware"/> to the specified application builder.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokRequestLogging(this IApplicationBuilder builder)
            => builder.UseMiddleware<LoggingMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="LoggingMiddleware"/> to the specified application builder.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokRequestLogging(this IApplicationBuilder builder, Action<LoggingSettings> setup)
            => builder.UseMiddleware<LoggingMiddleware>(Configure(setup));

        #endregion

        #region Helpers

        private static IOptions<TSettings> Configure<TSettings>(Action<TSettings> setup)
          where TSettings : class, new()
        {
            var settings = new TSettings();
            setup(settings);
            return Options.Create(settings);
        }

        #endregion
    }
}
