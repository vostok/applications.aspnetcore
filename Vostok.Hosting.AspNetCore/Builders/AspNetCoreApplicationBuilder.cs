using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Commons.Helpers;
using Vostok.Configuration.Microsoft;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Setup;
using Vostok.Hosting.AspNetCore.StartupFilters;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class AspNetCoreApplicationBuilder : IVostokAspNetCoreApplicationBuilder
    {
        private readonly LoggingMiddlewareBuilder loggingMiddlewareBuilder;
        private readonly TracingMiddlewareBuilder tracingMiddlewareBuilder;
        private readonly FillRequestInfoMiddlewareBuilder fillRequestInfoMiddlewareBuilder;
        private readonly RestoreDistributedContextMiddlewareBuilder restoreDistributedContextMiddlewareBuilder;
        private readonly DenyRequestsMiddlewareBuilder denyRequestsMiddlewareBuilder;
        private readonly PingApiMiddlewareBuilder pingApiMiddlewareBuilder;
        private readonly MicrosoftLogBuilder microsoftLogBuilder;
        private readonly Customization<IWebHostBuilder> webHostBuilderCustomization;

        public AspNetCoreApplicationBuilder()
        {
            loggingMiddlewareBuilder = new LoggingMiddlewareBuilder();
            tracingMiddlewareBuilder = new TracingMiddlewareBuilder();
            fillRequestInfoMiddlewareBuilder = new FillRequestInfoMiddlewareBuilder();
            restoreDistributedContextMiddlewareBuilder = new RestoreDistributedContextMiddlewareBuilder();
            denyRequestsMiddlewareBuilder = new DenyRequestsMiddlewareBuilder();
            pingApiMiddlewareBuilder = new PingApiMiddlewareBuilder();
            microsoftLogBuilder = new MicrosoftLogBuilder();
            webHostBuilderCustomization = new Customization<IWebHostBuilder>();
        }

        // CR(iloktionov): 2. Не вижу здесь возможности переопределить DI-контейнер (сделать так, чтобы IServiceProvider был на основе любимого контейнера разработчика).
        // CR(iloktionov): 4. Тут можно настраивать UseShutdownTimeout (время на drain запросов). Может, будем настраивать? Что там по умолчанию?
        // CR(iloktionov): 5. А есть смысл положить environment из нашей application identity в environment здесь, или это что-то сломает?
        public IHost Build(IVostokHostingEnvironment environment)
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging(
                    loggingBuilder => loggingBuilder
                        .ClearProviders().AddProvider(microsoftLogBuilder.Build(environment)))
                .ConfigureAppConfiguration(
                    configurationBuilder => configurationBuilder
                        .AddVostok(environment.ConfigurationSource)
                        .AddVostok(environment.SecretConfigurationSource))
                .ConfigureWebHostDefaults(
                    webHostBuilder =>
                    {
                        ConfigureUrl(webHostBuilder, environment);
                        var urlsBefore = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);

                        RegisterTypes(webHostBuilder, environment);

                        AddMiddlewares(webHostBuilder,
                            fillRequestInfoMiddlewareBuilder.Build(environment),
                            // TODO(kungurtsev): throttling middleware should go here.
                            restoreDistributedContextMiddlewareBuilder.Build(environment),
                            tracingMiddlewareBuilder.Build(environment),
                            loggingMiddlewareBuilder.Build(environment),
                            denyRequestsMiddlewareBuilder.Build(environment),
                            pingApiMiddlewareBuilder.Build(environment));

                        webHostBuilder.UseKestrel().UseSockets();

                        webHostBuilderCustomization.Customize(webHostBuilder);

                        var urlsAfter = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);
                        EnsureUrlsNotChanged(urlsBefore, urlsAfter);
                    });


            return hostBuilder.Build();
        }

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
        
        private static void ConfigureUrl(IWebHostBuilder builder, IVostokHostingEnvironment environment)
        {
            if (!environment.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url))
                throw new Exception("Port or url should be configured in ServiceBeacon using VostokHostingEnvironmentSetup.");

            builder = builder.UseUrls($"{url.Scheme}://*:{url.Port}/");

            AddStartupFilter(builder, new UrlPathStartupFilter(environment));
        }

        public static void AddMiddlewares(IWebHostBuilder builder, params IMiddleware[] middlewares)
        {
            middlewares = middlewares.Where(m => m != null).ToArray();

            foreach (var middleware in middlewares)
                builder.ConfigureServices(services => services.AddSingleton(middleware.GetType(), middleware));

            AddStartupFilter(builder, new AddMiddlewaresStartupFilter(
                middlewares.Select(m => m.GetType()).ToArray()));
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
            builder.ConfigureServices(
                services =>
                    services
                        .AddTransient(_ => startupFilter));

        #region SetupComponents

        public IVostokAspNetCoreApplicationBuilder SetupWebHost(Action<IWebHostBuilder> setup)
        {
            webHostBuilderCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupLoggingMiddleware(Action<IVostokLoggingMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(loggingMiddlewareBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupTracingMiddleware(Action<IVostokTracingMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(tracingMiddlewareBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder AllowRequestsIfNotInActiveDatacenter()
        {
            denyRequestsMiddlewareBuilder.AllowRequestsIfNotInActiveDatacenter();
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder DenyRequestsIfNotInActiveDatacenter(int denyResponseCode)
        {
            denyRequestsMiddlewareBuilder.DenyRequestsIfNotInActiveDatacenter(denyResponseCode);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupPingApiMiddleware(Action<IVostokPingApiMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            pingApiMiddlewareBuilder.Enable();
            setup(pingApiMiddlewareBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupFillRequestInfoMiddleware(Action<IVostokFillRequestInfoMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(fillRequestInfoMiddlewareBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupRestoreDistributedContextMiddleware(Action<IVostokRestoreDistributedContextMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(restoreDistributedContextMiddlewareBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog(Action<IVostokMicrosoftLogBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(microsoftLogBuilder);
            return this;
        }

        #endregion
    }
}