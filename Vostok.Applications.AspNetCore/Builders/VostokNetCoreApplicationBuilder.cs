using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Helpers;
using Vostok.Configuration.Microsoft;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokNetCoreApplicationBuilder : IVostokNetCoreApplicationBuilder
    {
        private readonly IVostokHostingEnvironment environment;

        private readonly Customization<IHostBuilder> genericHostCustomization;
        private readonly Customization<VostokLoggerProviderSettings> microsoftLogCustomization;

        public VostokNetCoreApplicationBuilder(IVostokHostingEnvironment environment)
        {
            this.environment = environment;

            genericHostCustomization = new Customization<IHostBuilder>();
            microsoftLogCustomization = new Customization<VostokLoggerProviderSettings>();
        }

        public IHost Build() =>
            CreateHostBuilder().Build();

        public IHostBuilder CreateHostBuilder()
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

            hostBuilder.ConfigureServices(
                services => services.AddSingleton<IHostLifetime, EmptyHostLifetime>());

            RegisterTypes(hostBuilder, environment);

            genericHostCustomization.Customize(new HostBuilderWrapper(hostBuilder));

            return hostBuilder;
        }

        public IVostokNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
        {
            genericHostCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokNetCoreApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
        {
            microsoftLogCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
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

        private ILoggerProvider CreateMicrosoftLog() =>
            new VostokLoggerProvider(environment.Log, microsoftLogCustomization.Customize(new VostokLoggerProviderSettings()));
    }
}