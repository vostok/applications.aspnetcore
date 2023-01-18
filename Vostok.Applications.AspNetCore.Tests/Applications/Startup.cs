using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Tests.Extensions;

namespace Vostok.Applications.AspNetCore.Tests.Applications
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
#if NETCOREAPP
            services.ConfigureTestsDefaults();
#else
            services.AddMvc();
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
#if NETCOREAPP
            app.ConfigureTestsDefaults();
#else
            app.UseMvc();
#endif
        }
    }
}