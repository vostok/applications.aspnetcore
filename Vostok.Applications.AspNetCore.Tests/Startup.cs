using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
#if ASPNETCORE3_1
            services.AddControllers()
                .AddNewtonsoftJson();
#elif ASPNETCORE2_1
            services.AddMvc();
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
#if ASPNETCORE3_1
            app.UseRouting();
            app.UseEndpoints(s => s.MapControllers());
            app.UseHealthChecks("/health");
#elif ASPNETCORE2_1
            app.UseMvc();
#endif
        }
    }
}