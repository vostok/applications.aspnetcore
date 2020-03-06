using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests
{
    internal class TestHostBuilder : IHostBuilder
    {
        private readonly List<Action<IVostokAspNetCoreApplicationBuilder>> hostConfigurations;
        private readonly IVostokHostingEnvironment env;

        public TestHostBuilder(IVostokHostingEnvironment env)
        {
            hostConfigurations = new List<Action<IVostokAspNetCoreApplicationBuilder>>();
            this.env = env;
        }

        public IDictionary<object, object> Properties { get; } = ImmutableDictionary<object, object>.Empty;

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) =>
            Configure(b => b.SetupGenericHost(s => s.ConfigureHostConfiguration(configureDelegate)));

        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
            Configure(b => b.SetupGenericHost(s => s.ConfigureAppConfiguration(configureDelegate)));

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
            Configure(b => b.SetupGenericHost(s => s.ConfigureServices(configureDelegate)));

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) =>
            throw new NotImplementedException();

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) =>
            throw new NotImplementedException();

        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) =>
            throw new NotImplementedException();

        public IHost Build()
        {
            var app = new TestVostokAspNetCoreApplication(hostConfigurations);

            app.Initialize(env);

            return app.Manager.Host;
        }

        private IHostBuilder Configure(Action<IVostokAspNetCoreApplicationBuilder> configureDelegate)
        {
            hostConfigurations.Add(configureDelegate);
            return this;
        }
    }
}