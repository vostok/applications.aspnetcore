#if NET6_0_OR_GREATER
using System;
using System.Threading.Tasks;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
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
        builder.SetupWebApplication(b => b.Services.ConfigureTestsDefaults());

        builder.CustomizeWebApplication(a => a.ConfigureTestsDefaults());

        configure(builder, environment);

        return Task.CompletedTask;
    }
}
#endif