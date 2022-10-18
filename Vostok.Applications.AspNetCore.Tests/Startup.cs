using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Tests.Extensions;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
#if NETCOREAPP
            services.ConfigureServiceCollection();
#else
            services.AddMvc();
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
#if NETCOREAPP
            app.ConfigureWebApplication();
#else
            app.UseMvc();
#endif
        }
    }
}