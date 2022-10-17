#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal class WebApplicationFactory
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly IVostokApplication application; 
        private readonly WebApplicationCustomizer webApplicationCustomizer;

        public WebApplicationFactory(IVostokHostingEnvironment environment,
                                     IVostokApplication application,
                                     WebApplicationCustomizer webApplicationCustomizer
            )
        {
            this.environment = environment;
            this.application = application;
            this.webApplicationCustomizer = webApplicationCustomizer;
        }

        public WebApplication Create()
        {
            var builder = CreateBuilder();

            var webApplication = builder.Build();
            
            webApplicationCustomizer.CustomizeWebApplication(webApplication);

            return webApplication;
        }

        private WebApplicationBuilder CreateBuilder()
        {
            var builder = WebApplication.CreateBuilder(
                webApplicationCustomizer.CustomizeWebApplicationOptions(new WebApplicationOptions()));

            builder.Services
                .AddSingleton<IHostLifetime, GenericHostEmptyLifetime>()
                .AddVostokEnvironment(environment, application);

            webApplicationCustomizer.CustomizeWebApplicationBuilder(builder);
            
            // builder.Services
            //     .AddSingleton<IHostLifetime, GenericHostEmptyLifetime>()
            //     .AddVostokEnvironment(environment, application);
            //
            return builder;
        }
    }
}
#endif