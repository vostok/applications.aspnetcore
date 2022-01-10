using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
#if NETCOREAPP
            services.AddControllers()
                .AddNewtonsoftJson();
#else
            services.AddMvc();
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
#if NETCOREAPP
            app.UseRouting();
            app.UseEndpoints(s => s.MapControllers());
            app.UseHealthChecks("/health");
#else
            app.UseMvc();
#endif
        }
    }
}