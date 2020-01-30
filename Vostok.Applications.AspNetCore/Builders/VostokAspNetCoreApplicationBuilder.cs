using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Applications.AspNetCore.StartupFilters;
using Vostok.Commons.Helpers;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;
using Vostok.Configuration.Microsoft;
using Vostok.Context;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Throttling.Metrics;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokAspNetCoreApplicationBuilder<TStartup> : IVostokAspNetCoreApplicationBuilder
        where TStartup : class
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly List<IDisposable> disposables;
        private readonly AtomicBoolean initialized;
        private readonly AtomicBoolean webHostEnabled;
        private readonly Customization<IHostBuilder> genericHostCustomization;
        private readonly Customization<IWebHostBuilder> webHostBuilderCustomization;
        private readonly Customization<KestrelSettings> kestrelCustomization;
        private readonly Customization<TracingSettings> tracingCustomization;
        private readonly Customization<LoggingSettings> loggingCustomization;
        private readonly Customization<PingApiSettings> pingApiCustomization;
        private readonly Customization<FillRequestInfoSettings> fillRequestInfoCustomization;
        private readonly Customization<VostokLoggerProviderSettings> microsoftLogCustomization;
        private readonly Customization<DistributedContextSettings> distributedContextCustomization;
        private readonly Customization<DatacenterAwarenessSettings> datacenterAwarenessCustomization;
        private readonly VostokThrottlingBuilder throttlingBuilder;

        public VostokAspNetCoreApplicationBuilder(IVostokHostingEnvironment environment, List<IDisposable> disposables, AtomicBoolean initialized)
        {
            this.environment = environment;
            this.disposables = disposables;
            this.initialized = initialized;

            webHostEnabled = true;
            webHostBuilderCustomization = new Customization<IWebHostBuilder>();
            genericHostCustomization = new Customization<IHostBuilder>();
            kestrelCustomization = new Customization<KestrelSettings>();
            tracingCustomization = new Customization<TracingSettings>();
            loggingCustomization = new Customization<LoggingSettings>();
            pingApiCustomization = new Customization<PingApiSettings>();
            fillRequestInfoCustomization = new Customization<FillRequestInfoSettings>();
            microsoftLogCustomization = new Customization<VostokLoggerProviderSettings>();
            distributedContextCustomization = new Customization<DistributedContextSettings>();
            datacenterAwarenessCustomization = new Customization<DatacenterAwarenessSettings>();
            throttlingBuilder = new VostokThrottlingBuilder(environment);
        }

        public IHost Build()
        {
            using (FlowingContext.Globals.Use(environment))
            {
                var hostBuilder = Host.CreateDefaultBuilder();

                hostBuilder.ConfigureLogging(
                    loggingBuilder => loggingBuilder
                        .ClearProviders()
                        .AddProvider(CreateMicrosoftLog()));

                hostBuilder.ConfigureAppConfiguration(
                    configurationBuilder => configurationBuilder
                        .AddVostok(environment.ConfigurationSource)
                        .AddVostok(environment.SecretConfigurationSource));

                RegisterTypes(hostBuilder, environment);

                genericHostCustomization.Customize(new HostBuilderWrapper(hostBuilder));

                if (webHostEnabled)
                {
                    hostBuilder.ConfigureWebHost(
                        webHostBuilder =>
                        {
                            ConfigureUrl(webHostBuilder, environment);
                            var urlsBefore = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);

                            AddMiddlewares(
                                webHostBuilder,
                                CreateFillRequestInfoMiddleware(),
                                CreateDistributedContextMiddleware(),
                                CreateTracingMiddleware(),
                                CreateThrottlingMiddleware(),
                                CreateLoggingMiddleware(),
                                CreateDatacenterAwarenessMiddleware(),
                                CreateErrorHandlingMiddleware(),
                                CreatePingApiMiddleware());

                            webHostBuilder.UseKestrel(ConfigureKestrel);
                            webHostBuilder.UseSockets(ConfigureSocketTransport);
                            webHostBuilder.UseShutdownTimeout(environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));

                            if (typeof(TStartup) != typeof(EmptyStartup))
                                webHostBuilder.UseStartup<TStartup>();

                            webHostBuilderCustomization.Customize(webHostBuilder);

                            var urlsAfter = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);
                            EnsureUrlsNotChanged(urlsBefore, urlsAfter);
                        });
                }

                return hostBuilder.Build();
            }
        }

        private static void AddMiddlewares(IWebHostBuilder builder, params IMiddleware[] middlewares)
        {
            foreach (var middleware in middlewares)
                builder.ConfigureServices(services => services.AddSingleton(middleware.GetType(), middleware));

            AddStartupFilter(
                builder,
                new AddMiddlewaresStartupFilter(
                    middlewares.Select(m => m.GetType()).ToArray()));
        }

        private static void ConfigureUrl(IWebHostBuilder builder, IVostokHostingEnvironment environment)
        {
            if (!environment.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url))
                throw new Exception("Port or url should be configured in ServiceBeacon using VostokHostingEnvironmentSetup.");

            builder = builder.UseUrls($"{url.Scheme}://*:{url.Port}/");

            AddStartupFilter(builder, new UrlPathStartupFilter(environment));
        }

        private static void RegisterTypes(IHostBuilder builder, IVostokHostingEnvironment environment) =>
            builder.ConfigureServices(
                services =>
                {
                    services
                        .AddSingleton(environment)
                        .AddSingleton(environment.ApplicationIdentity)
                        .AddSingleton(environment.ApplicationLimits)
                        .AddTransient(_ => environment.ApplicationReplicationInfo)
                        .AddSingleton(environment.Metrics)
                        .AddSingleton(environment.Log)
                        .AddSingleton(environment.Tracer)
                        .AddSingleton(environment.HerculesSink)
                        .AddSingleton(environment.ConfigurationSource)
                        .AddSingleton(environment.ConfigurationProvider)
                        .AddSingleton(environment.ClusterConfigClient)
                        .AddSingleton(environment.ServiceBeacon)
                        .AddSingleton(environment.ServiceLocator)
                        .AddSingleton(environment.ContextGlobals)
                        .AddSingleton(environment.ContextProperties)
                        .AddSingleton(environment.ContextConfiguration)
                        .AddSingleton(environment.Datacenters)
                        .AddSingleton(environment.HostExtensions);

                    foreach (var (type, obj) in environment.HostExtensions.GetAll())
                    {
                        services.AddSingleton(type, obj);
                    }
                });

        private static void AddStartupFilter(IWebHostBuilder builder, IStartupFilter startupFilter) =>
            builder.ConfigureServices(services => services.AddTransient(_ => startupFilter));

        private static void ConfigureSocketTransport(SocketTransportOptions options)
            => options.NoDelay = true;

        private void EnsureUrlsNotChanged(string urlsBefore, string urlsAfter)
        {
            if (urlsAfter.Contains(urlsBefore))
                return;

            throw new Exception(
                "Application url should be configured in ServiceBeacon instead of WebHostBuilder.\n" +
                $"ServiceBeacon url: '{urlsBefore}'. WebHostBuilder urls: '{urlsAfter}'.\n" +
                "To configure application port (without url) use VostokHostingEnvironmentSetup extension: `vostokHostingEnvironmentSetup.SetPort(...)`.\n" +
                "To configure application url use VostokHostingEnvironmentSetup: `vostokHostingEnvironmentSetup.SetupServiceBeacon(serviceBeaconBuilder => serviceBeaconBuilder.SetupReplicaInfo(replicaInfo => replicaInfo.SetUrl(...)))`.");
        }

        private void ConfigureKestrel(KestrelServerOptions options)
        {
            var settings = kestrelCustomization.Customize(new KestrelSettings());

            options.AddServerHeader = false;
            options.AllowSynchronousIO = false;

            options.Limits.MaxConcurrentConnections = null;
            options.Limits.MaxRequestBufferSize = 256 * 1024;
            options.Limits.MaxResponseBufferSize = 256 * 1024;
            options.Limits.Http2.MaxStreamsPerConnection = 1000;

            options.Limits.MaxRequestBodySize = settings.MaxRequestBodySize;
            options.Limits.MaxRequestLineSize = settings.MaxRequestLineSize;
            options.Limits.MaxRequestHeadersTotalSize = settings.MaxRequestHeadersSize;
            options.Limits.MaxConcurrentUpgradedConnections = settings.MaxConcurrentWebSocketConnections;

            if (settings.KeepAliveTimeout.HasValue)
                options.Limits.KeepAliveTimeout = settings.KeepAliveTimeout.Value;

            if (settings.RequestHeadersTimeout.HasValue)
                options.Limits.RequestHeadersTimeout = settings.RequestHeadersTimeout.Value;
        }

        #region CreateComponents

        private IMiddleware CreateDatacenterAwarenessMiddleware()
            => new DatacenterAwarenessMiddleware(datacenterAwarenessCustomization.Customize(new DatacenterAwarenessSettings()), environment.Datacenters, environment.Log);

        private IMiddleware CreateFillRequestInfoMiddleware()
            => new FillRequestInfoMiddleware(fillRequestInfoCustomization.Customize(new FillRequestInfoSettings()));

        private IMiddleware CreateDistributedContextMiddleware()
            => new DistributedContextMiddleware(distributedContextCustomization.Customize(new DistributedContextSettings()));

        private IMiddleware CreateTracingMiddleware()
            => new TracingMiddleware(tracingCustomization.Customize(new TracingSettings()), environment.Tracer);

        private IMiddleware CreateLoggingMiddleware()
            => new LoggingMiddleware(loggingCustomization.Customize(new LoggingSettings()), environment.Log);

        private IMiddleware CreatePingApiMiddleware()
            => new PingApiMiddleware(pingApiCustomization.Customize(new PingApiSettings()), initialized);

        private IMiddleware CreateErrorHandlingMiddleware()
            => new UnhandledErrorMiddleware(environment.Log);

        private ILoggerProvider CreateMicrosoftLog()
        {
            var settings = new VostokLoggerProviderSettings
            {
                IgnoredScopes = new HashSet<string>
                {
                    MicrosoftConstants.ActionLogScope,
                    MicrosoftConstants.HostingLogScope,
                    MicrosoftConstants.ConnectionLogScope
                }
            };

            return new VostokLoggerProvider(environment.Log, microsoftLogCustomization.Customize(settings));
        }

        private IMiddleware CreateThrottlingMiddleware()
        {
            var (provider, settings) = throttlingBuilder.Build();

            if (settings.Metrics != null)
                disposables.Add(environment.Metrics.Instance.CreateThrottlingMetrics(provider, settings.Metrics));

            return new ThrottlingMiddleware(settings, provider, environment.Log);
        }

        #endregion

        #region SetupComponents

        public IVostokAspNetCoreApplicationBuilder DisableWebHost()
        {
            webHostEnabled.Value = false;
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
        {
            genericHostCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupWebHost(Action<IWebHostBuilder> setup)
        {
            webHostBuilderCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupKestrel(Action<KestrelSettings> setup)
        {
            kestrelCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupDatacenterAwareness(Action<DatacenterAwarenessSettings> setup)
        {
            datacenterAwarenessCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupRequestInfoFilling(Action<FillRequestInfoSettings> setup)
        {
            fillRequestInfoCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupDistributedContext(Action<DistributedContextSettings> setup)
        {
            distributedContextCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupTracing(Action<TracingSettings> setup)
        {
            tracingCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupLogging(Action<LoggingSettings> setup)
        {
            loggingCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupPingApi(Action<PingApiSettings> setup)
        {
            pingApiCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
        {
            microsoftLogCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupThrottling(Action<IVostokThrottlingBuilder> setup)
        {
            setup(throttlingBuilder);
            return this;
        }

        #endregion
    }
}