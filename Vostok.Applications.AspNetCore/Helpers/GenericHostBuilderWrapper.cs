#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class GenericHostBuilderWrapper : IHostBuilder
    {
        private readonly IHostBuilder builder;

        public GenericHostBuilderWrapper(IHostBuilder builder)
            => this.builder = builder;

        public IDictionary<object, object> Properties
            => builder.Properties;

        public IHost Build()
            => throw new NotSupportedException("Vostok application builder does not permit this operation.");

        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
            builder.ConfigureAppConfiguration(configureDelegate);

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) =>
            builder.ConfigureHostConfiguration(configureDelegate);

        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) =>
            builder.ConfigureContainer(configureDelegate);

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
            builder.ConfigureServices(configureDelegate);

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) =>
            builder.UseServiceProviderFactory(factory);

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) =>
            builder.UseServiceProviderFactory(factory);
    }
}
#endif