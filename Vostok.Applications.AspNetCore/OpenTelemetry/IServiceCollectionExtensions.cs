#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Diagnostics;
using EnvironmentInfo = Vostok.Commons.Environment.EnvironmentInfo;

namespace Vostok.Applications.AspNetCore.OpenTelemetry;

[PublicAPI]
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// <para>Adds default Vostok tags:</para>
    /// <list type="bullet">
    ///     <item><description><see cref="SemanticConventions.AttributeClientAddress"/></description></item>
    ///     <item><description><see cref="WellKnownAnnotations.Http.Client.Name"/></description></item>
    ///     <item><description><see cref="SemanticConventions.AttributeHttpRequestContentLength"/></description></item>
    ///     <item><description><see cref="SemanticConventions.AttributeHttpResponseContentLength"/></description></item>
    /// </list>
    /// </summary>
    public static IServiceCollection ConfigureVostokAspNetCoreInstrumentation(this IServiceCollection serviceCollection, string name = null)
    {
        name ??= Options.DefaultName;
        serviceCollection.Configure<AspNetCoreTraceInstrumentationOptions>(name, Enrich);
        return serviceCollection;

        static void Enrich(AspNetCoreTraceInstrumentationOptions options)
        {
            var enrichWithHttpRequest = options.EnrichWithHttpRequest;
            options.EnrichWithHttpRequest = (activity, request) =>
            {
                enrichWithHttpRequest?.Invoke(activity, request);
                activity.SetTag(SemanticConventions.AttributeClientAddress, request.HttpContext.Connection.RemoteIpAddress);
                var clientName = request.Headers[HeaderNames.ApplicationIdentity].ToString();
                if (!string.IsNullOrEmpty(clientName))
                    activity.SetTag(WellKnownAnnotations.Http.Client.Name, clientName);
                if (request.ContentLength.HasValue)
                    activity.SetTag(SemanticConventions.AttributeHttpRequestContentLength, request.ContentLength.Value);
            };

            var enrichWithHttpResponse = options.EnrichWithHttpResponse;
            options.EnrichWithHttpResponse = (activity, response) =>
            {
                enrichWithHttpResponse?.Invoke(activity, response);
                if (response.ContentLength.HasValue)
                    activity.SetTag(SemanticConventions.AttributeHttpResponseContentLength, response.ContentLength.Value);
            };
        }
    }

    /// <summary>
    /// <para>Adds <see cref="TracingConstants.VostokTracerActivitySourceName"/> <see cref="ActivitySource"/>.</para>
    /// <para>Adds Vostok identity <see cref="Resource"/>.</para>
    /// </summary>
    public static IServiceCollection ConfigureVostokOpenTelemetryTracerProvider(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureOpenTelemetryTracerProvider(
            (services, tracerProviderBuilder) => tracerProviderBuilder
                .ConfigureResource(resourceBuilder => ConfigureTracerResource(resourceBuilder, services))
                .AddSource(TracingConstants.VostokTracerActivitySourceName));

        return serviceCollection;
    }

    /// <summary>
    /// <para>Adds Vostok identity <see cref="Resource"/>.</para>
    /// </summary>
    public static IServiceCollection ConfigureVostokOpenTelemetryMeterProvider(this IServiceCollection serviceCollection)
    {
        var name = Options.DefaultName;

        serviceCollection.ConfigureOpenTelemetryMeterProvider(
            (services, tracerProviderBuilder) => tracerProviderBuilder
                .SetResourceBuilder(ResourceBuilder.CreateEmpty())
                .ConfigureResource(resourceBuilder => ConfigureMeterResource(resourceBuilder, services, name)));

        return serviceCollection;
    }

    private static void ConfigureTracerResource(ResourceBuilder resourceBuilder, IServiceProvider services)
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
            new(SemanticConventions.AttributeHostName, host)
        };
        if (environment != null)
            vostokTags.Add(new(SemanticConventions.AttributeDeploymentEnvironmentName, environment));

        resourceBuilder.AddAttributes(vostokTags);
    }

    private static void ConfigureMeterResource(ResourceBuilder resourceBuilder, IServiceProvider services, string name)
    {
        var options = services.GetRequiredService<IOptionsMonitor<VostokOpenTelemetryMeterProviderOptions>>().Get(name);

        if (options.AddService)
        {
            var application = ClusterClientDefaults.ClientApplicationName;
            var serviceBeacon = services.GetService<IServiceBeacon>();
            if (serviceBeacon != null)
            {
                application = serviceBeacon.ReplicaInfo.Application;
            }

            resourceBuilder.AddService(serviceName: application, autoGenerateServiceInstanceId: false);
        }

        var identity = services.GetService<IVostokApplicationIdentity>();
        var vostokTags = new List<KeyValuePair<string, object>>();
        if (options.AddProject)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Project, identity.Project));
        if (options.AddSubproject && identity.Subproject != null)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Subproject, identity.Subproject));
        if (options.AddEnvironment)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Environment, identity.Environment));
        if (options.AddApplication)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Application, identity.Application));

        if (options.AddInstance)
        {
            var instance = identity.Instance;
            if (string.Equals(instance, EnvironmentInfo.Host, StringComparison.InvariantCultureIgnoreCase))
                instance = instance.ToLowerInvariant();
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Instance, instance));
        }

        resourceBuilder.AddAttributes(vostokTags);
    }
}
#endif