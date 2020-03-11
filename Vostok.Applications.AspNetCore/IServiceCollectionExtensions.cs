using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore
{
    [PublicAPI]
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="DistributedContextSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokDistributedContext(this IServiceCollection services)
            => services.AddVostokDistributedContext(_ => {});

        /// <summary>
        /// Adds <see cref="DistributedContextSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokDistributedContext(this IServiceCollection services, Action<DistributedContextSettings> setup)
            => services.Configure(setup);

        /// <summary>
        /// Adds <see cref="DatacenterAwarenessSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokDatacenterAwareness(this IServiceCollection services)
            => services.AddVostokDatacenterAwareness(_ => {});

        /// <summary>
        /// Adds <see cref="DatacenterAwarenessSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokDatacenterAwareness(this IServiceCollection services, Action<DatacenterAwarenessSettings> setup)
            => services.Configure(setup);

        /// <summary>
        /// Adds <see cref="FillRequestInfoSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokRequestInfo(this IServiceCollection services)
            => services.AddVostokRequestInfo(_ => { });

        /// <summary>
        /// Adds <see cref="FillRequestInfoSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokRequestInfo(this IServiceCollection services, Action<FillRequestInfoSettings> setup)
            => services.Configure(setup);

        /// <summary>
        /// Adds <see cref="TracingSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokTracing(this IServiceCollection services)
            => services.AddVostokTracing(_ => {});

        /// <summary>
        /// Adds <see cref="TracingSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokTracing(this IServiceCollection services, Action<TracingSettings> setup)
            => services.Configure(setup);

        /// <summary>
        /// Adds <see cref="ThrottlingSettings"/> and <see cref="IThrottlingProvider"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokThrottling(this IServiceCollection services, IThrottlingProvider provider)
            => services.AddVostokThrottling(provider, _ => {});

        /// <summary>
        /// Adds <see cref="ThrottlingSettings"/> and <see cref="IThrottlingProvider"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokThrottling(this IServiceCollection services, IThrottlingProvider provider, Action<ThrottlingSettings> setup)
            => services
                .Configure(setup)
                .AddSingleton(provider);

        /// <summary>
        /// Adds <see cref="PingApiSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokPingApi(this IServiceCollection services)
            => services.AddVostokPingApi(_ => {});

        /// <summary>
        /// Adds <see cref="PingApiSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokPingApi(this IServiceCollection services, Action<PingApiSettings> setup)
            => services.Configure(setup);

        /// <summary>
        /// Adds <see cref="LoggingSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokRequestLogging(this IServiceCollection services)
            => services.AddVostokRequestLogging(_ => {});

        /// <summary>
        /// Adds <see cref="LoggingSettings"/> to the specified service collection.
        /// </summary>
        public static IServiceCollection AddVostokRequestLogging(this IServiceCollection services, Action<LoggingSettings> setup)
            => services.Configure(setup);
    }
}
