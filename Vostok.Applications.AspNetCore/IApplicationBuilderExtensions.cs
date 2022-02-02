using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore
{
    [PublicAPI]
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// <para>Adds the <see cref="DistributedContextMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokDistributedContext(IServiceCollection,System.Action{Vostok.Applications.AspNetCore.Configuration.DistributedContextSettings})"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokDistributedContext(this IApplicationBuilder builder)
            => builder.UseMiddleware<DistributedContextMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="DatacenterAwarenessMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokDatacenterAwareness(IServiceCollection,Action{DatacenterAwarenessSettings})"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokDatacenterAwareness(this IApplicationBuilder builder)
            => builder.UseMiddleware<DatacenterAwarenessMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="FillRequestInfoMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokRequestInfo(IServiceCollection,Action{FillRequestInfoSettings})"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokRequestInfo(this IApplicationBuilder builder)
            => builder.UseMiddleware<FillRequestInfoMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="TracingMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokTracing(IServiceCollection,Action{TracingSettings})"/> to register its options.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/> and <see cref="DistributedContextMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokTracing(this IApplicationBuilder builder)
            => builder.UseMiddleware<TracingMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="ThrottlingMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokThrottling(IServiceCollection,IThrottlingProvider,Action{ThrottlingSettings})"/> to register its options.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokThrottling(this IApplicationBuilder builder)
            => builder.UseMiddleware<ThrottlingMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="PingApiMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokPingApi(IServiceCollection,Action{PingApiSettings})"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokPingApi(this IApplicationBuilder builder)
            => builder.UseMiddleware<PingApiMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="DiagnosticApiMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokDiagnosticApi(IServiceCollection,Action{DiagnosticApiSettings})"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokDiagnosticApi(this IApplicationBuilder builder)
            => builder.UseMiddleware<DiagnosticApiMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="LoggingMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokRequestLogging(IServiceCollection,Action{LoggingSettings})"/> to register its options.</para>
        /// <para>Note that this middleware has a hard dependency on <see cref="FillRequestInfoMiddleware"/>.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokRequestLogging(this IApplicationBuilder builder)
            => builder.UseMiddleware<LoggingMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="HttpContextTweakMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokHttpContextTweaks(IServiceCollection,Action{HttpContextTweakSettings})"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokHttpContextTweaks(this IApplicationBuilder builder)
            => builder.UseMiddleware<HttpContextTweakMiddleware>();

        /// <summary>
        /// <para>Adds the <see cref="UnhandledExceptionMiddleware"/> to the specified application builder.</para>
        /// <para>Use <see cref="IServiceCollectionExtensions.AddVostokUnhandledExceptions(IServiceCollection,Action{UnhandledExceptionSettings})"/> to register its options.</para>
        /// </summary>
        public static IApplicationBuilder UseVostokUnhandledExceptions(this IApplicationBuilder builder)
            => builder.UseMiddleware<UnhandledExceptionMiddleware>();
    }
}