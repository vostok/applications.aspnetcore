#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Vostok.Clusterclient.Core;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Diagnostics;
using EnvironmentInfo = Vostok.Commons.Environment.EnvironmentInfo;

namespace Vostok.Applications.AspNetCore.Extensions;

[PublicAPI]
public static class IServiceCollectionExtensions_OpenTelemetry
{
    /// <summary>
    /// <para>Adds <see cref="TracingConstants.VostokTracerActivitySourceName"/> <see cref="ActivitySource"/>.</para>
    /// <para>Adds Vostok identity <see cref="Resource"/>.</para>
    /// </summary>
    public static IServiceCollection ConfigureVostokOpenTelemetryTracerProvider(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureOpenTelemetryTracerProvider(
            (services, tracerProviderBuilder) => tracerProviderBuilder
                .ConfigureResource(resourceBuilder => ConfigureResource(resourceBuilder, services))
                .AddSource(TracingConstants.VostokTracerActivitySourceName));

        return serviceCollection;
    }

    private static void ConfigureResource(ResourceBuilder resourceBuilder, IServiceProvider services)
    {
        var host = EnvironmentInfo.Host;
        var application = ClusterClientDefaults.ClientApplicationName;
        string environment = null;
        
        var serviceBeacon = services.GetService<IServiceBeacon>();
        if (serviceBeacon != null)
        {
            application = serviceBeacon.ReplicaInfo.Application;
            environment = serviceBeacon.ReplicaInfo.Environment;
        }
        
        resourceBuilder.AddService(serviceName: application, autoGenerateServiceInstanceId: false);
        var vostokTags = new List<KeyValuePair<string, object>>
        {
            new("host.name", host)
        };
        if (environment != null)
            vostokTags.Add(new("deployment.environment", environment));

        resourceBuilder.AddAttributes(vostokTags);
    }
}
#endif