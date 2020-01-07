using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Commons.Helpers;
using Vostok.Configuration.Microsoft;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.StartupFilters;
using Vostok.Logging.Microsoft;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Throttling.Metrics;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class VostokAspNetCoreApplicationBuilder<TStartup> : IVostokAspNetCoreApplicationBuilder
        where TStartup : class
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly List<IDisposable> disposables;
        private readonly Customization<IWebHostBuilder> webHostBuilderCustomization;
        private readonly Customization<TracingSettings> tracingCustomization;
        private readonly Customization<LoggingSettings> loggingCustomization;
        private readonly Customization<PingApiSettings> pingApiCustomization;
        private readonly Customization<FillRequestInfoSettings> fillRequestInfoCustomization;
        private readonly Customization<VostokLoggerProviderSettings> microsoftLogCustomization;
        private readonly Customization<DistributedContextSettings> distributedContextCustomization;
        private readonly Customization<DatacenterAwarenessSettings> datacenterAwarenessCustomization;
        private readonly VostokThrottlingBuilder throttlingBuilder;

        public VostokAspNetCoreApplicationBuilder(IVostokHostingEnvironment environment, List<IDisposable> disposables)
        {
            this.environment = environment;
            this.disposables = disposables;

            webHostBuilderCustomization = new Customization<IWebHostBuilder>();
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
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging(
                    loggingBuilder => loggingBuilder
                        .ClearProviders()
                        .AddProvider(CreateMicrosoftLog()))
                .ConfigureAppConfiguration(
                    configurationBuilder => configurationBuilder
                        .AddVostok(environment.ConfigurationSource)
                        .AddVostok(environment.SecretConfigurationSource))
                .ConfigureWebHost(
                    webHostBuilder =>
                    {
                        ConfigureUrl(webHostBuilder, environment);
                        var urlsBefore = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);

                        RegisterTypes(webHostBuilder, environment);

                        AddMiddlewares(
                            webHostBuilder,
                            CreateFillRequestInfoMiddleware(),
                            CreateDistributedContextMiddleware(),
                            CreateTracingMiddleware(),
                            CreateThrottlingMiddleware(),
                            CreateLoggingMiddleware(),
                            CreateDatacenterAwarenessMiddleware(),
                            CreatePingApiMiddleware());

                        webHostBuilder.UseKestrel().UseSockets();
                        webHostBuilder.UseShutdownTimeout(environment.ShutdownTimeout);
                        webHostBuilder.UseStartup<TStartup>();
                        webHostBuilderCustomization.Customize(webHostBuilder);

                        var urlsAfter = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);
                        EnsureUrlsNotChanged(urlsBefore, urlsAfter);
                    });

            return hostBuilder.Build();
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

        private static void RegisterTypes(IWebHostBuilder builder, IVostokHostingEnvironment environment) =>
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
            => new PingApiMiddleware(pingApiCustomization.Customize(new PingApiSettings()));

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

        public IVostokAspNetCoreApplicationBuilder SetupWebHost(Action<IWebHostBuilder> setup)
        {
            webHostBuilderCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
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