using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore
{
    [PublicAPI]
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// <para>Adds the <see cref="DistributedContextMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokDistributedContext(IServiceCollection)"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokDistributedContext(this IApplicationBuilder builder)
            => builder.UseMiddleware<DistributedContextMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="DatacenterAwarenessMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokDatacenterAwareness(IServiceCollection)"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokDatacenterAwareness(this IApplicationBuilder builder)
            => builder.UseMiddleware<DatacenterAwarenessMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="FillRequestInfoMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokRequestInfo(IServiceCollection)"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokRequestInfo(this IApplicationBuilder builder)
            => builder.UseMiddleware<FillRequestInfoMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="TracingMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokTracing(IServiceCollection)"/> to register its options.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/> and <see cref="DistributedContextMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokTracing(this IApplicationBuilder builder)
            => builder.UseMiddleware<TracingMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="ThrottlingMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokThrottling(IServiceCollection,IThrottlingProvider)"/> to register its options.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokThrottling(this IApplicationBuilder builder)
            => builder.UseMiddleware<ThrottlingMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="PingApiMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokPingApi(IServiceCollection)"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokPingApi(this IApplicationBuilder builder)
            => builder.UseMiddleware<PingApiMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="LoggingMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokRequestLogging(IServiceCollection)"/> to register its options.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokRequestLogging(this IApplicationBuilder builder)
            => builder.UseMiddleware<LoggingMiddleware>();
    }
}
