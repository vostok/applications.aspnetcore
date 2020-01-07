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
using Vostok.Throttling;
using Vostok.Throttling.Config;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class VostokAspNetCoreApplicationBuilder<TStartup> : IVostokAspNetCoreApplicationBuilder
        where TStartup : class
    {
        private readonly Customization<IWebHostBuilder> webHostBuilderCustomization;
        private readonly Customization<TracingSettings> tracingCustomization;
        private readonly Customization<LoggingSettings> loggingCustomization;
        private readonly Customization<PingApiSettings> pingApiCustomization;
        private readonly Customization<ThrottlingSettings> throttlingCustomization;
        private readonly Customization<FillRequestInfoSettings> fillRequestInfoCustomization;
        private readonly Customization<VostokLoggerProviderSettings> microsoftLogCustomization;
        private readonly Customization<DistributedContextSettings> distributedContextCustomization;
        private readonly Customization<DatacenterAwarenessSettings> datacenterAwarenessCustomization;

        public VostokAspNetCoreApplicationBuilder()
        {
            webHostBuilderCustomization = new Customization<IWebHostBuilder>();
            tracingCustomization = new Customization<TracingSettings>();
            loggingCustomization = new Customization<LoggingSettings>();
            pingApiCustomization = new Customization<PingApiSettings>();
            throttlingCustomization = new Customization<ThrottlingSettings>();
            fillRequestInfoCustomization = new Customization<FillRequestInfoSettings>();
            microsoftLogCustomization = new Customization<VostokLoggerProviderSettings>();
            distributedContextCustomization = new Customization<DistributedContextSettings>();
            datacenterAwarenessCustomization = new Customization<DatacenterAwarenessSettings>();
        }

        public IHost Build(IVostokHostingEnvironment environment)
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging(
                    loggingBuilder => loggingBuilder
                        .ClearProviders()
                        .AddProvider(CreateMicrosoftLog(environment)))
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
                            CreateRestoreDistributedContextMiddleware(),
                            CreateTracingMiddleware(environment),
                            CreateThrottlingMiddleware(environment),
                            CreateLoggingMiddleware(environment),
                            CreateDenyRequestsIfNotInActiveDatacenterMiddleware(environment),
                            CreatePingApiMiddleware(environment));

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

        private IMiddleware CreateDenyRequestsIfNotInActiveDatacenterMiddleware(IVostokHostingEnvironment environment)
        {
            var settings = new DatacenterAwarenessSettings();

            datacenterAwarenessCustomization.Customize(settings);
            
            return new DatacenterAwarenessMiddleware(settings, environment.Datacenters, environment.Log);
        }

        private IMiddleware CreateFillRequestInfoMiddleware()
        {
            var settings = new FillRequestInfoSettings();

            fillRequestInfoCustomization.Customize(settings);

            return new FillRequestInfoMiddleware(settings);
        }

        private IMiddleware CreateRestoreDistributedContextMiddleware()
        {
            var settings = new DistributedContextSettings();

            distributedContextCustomization.Customize(settings);

            return new DistributedContextMiddleware(settings);
        }

        private IMiddleware CreateTracingMiddleware(IVostokHostingEnvironment environment)
        {
            var settings = new TracingSettings();

            tracingCustomization.Customize(settings);

            return new TracingMiddleware(settings, environment.Tracer);
        }

        private IMiddleware CreateLoggingMiddleware(IVostokHostingEnvironment environment)
        {
            var settings = new LoggingSettings();

            loggingCustomization.Customize(settings);

            return new LoggingMiddleware(settings, environment.Log);
        }

        private IMiddleware CreatePingApiMiddleware(IVostokHostingEnvironment environment)
        {
            var settings = new PingApiSettings();

            pingApiCustomization.Customize(settings);

            return new PingApiMiddleware(settings);
        }

        private ILoggerProvider CreateMicrosoftLog(IVostokHostingEnvironment environment)
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

            microsoftLogCustomization.Customize(settings);

            return new VostokLoggerProvider(environment.Log, settings);
        }

        // TODO(iloktionov): 1. Add throttling metrics
        // TODO(iloktionov): 2. Use custom cores count provider
        private IMiddleware CreateThrottlingMiddleware(IVostokHostingEnvironment environment)
        {
            var settings = new ThrottlingSettings();

            throttlingCustomization.Customize(settings);

            var configBuilder = new ThrottlingConfigurationBuilder();

            var throttlingProvider = new ThrottlingProvider(configBuilder.Build());

            return new ThrottlingMiddleware(settings, throttlingProvider, environment.Log);
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

        public IVostokAspNetCoreApplicationBuilder SetupFillRequestInfo(Action<FillRequestInfoSettings> setup)
        {
            fillRequestInfoCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupRestoreDistributedContext(Action<DistributedContextSettings> setup)
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

        public IVostokAspNetCoreApplicationBuilder SetupThrottling(Action<ThrottlingSettings> setup)
        {
            throttlingCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

            return this;
        }

        #endregion
    }
}