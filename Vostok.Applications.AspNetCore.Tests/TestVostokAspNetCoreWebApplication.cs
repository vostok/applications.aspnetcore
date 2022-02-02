#if NET6_0
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests;

public class TestVostokAspNetCoreWebApplication : VostokAspNetCoreWebApplication
{
    private readonly Action<IVostokAspNetCoreWebApplicationBuilder, IVostokHostingEnvironment> configure;

    public TestVostokAspNetCoreWebApplication(Action<IVostokAspNetCoreWebApplicationBuilder, IVostokHostingEnvironment> configure)
    {
        this.configure = configure;
    }

    public override Task SetupAsync(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
    {
        builder.SetupWebApplication(b => b.Services
            .AddControllers()
            .AddNewtonsoftJson()
            .AddApplicationPart(typeof(Startup).Assembly));

        builder.CustomizeWebApplication(a => a
            .UseRouting()
            .UseEndpoints(s => s.MapControllers())
            .UseHealthChecks("/health"));

        configure(builder, environment);

        return Task.CompletedTask;
    }
}
#endif