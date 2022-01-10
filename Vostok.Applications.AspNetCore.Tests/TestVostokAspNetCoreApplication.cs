using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests
{
#if NET6_0
    public class TestVostokAspNetCoreApplication : VostokAspNetCoreApplication
#else
    public class TestVostokAspNetCoreApplication : VostokAspNetCoreApplication<Startup>
#endif
    {
        private readonly Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> configure;

        public TestVostokAspNetCoreApplication(Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> configure)
        {
            this.configure = configure;
        }

        public override void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            configure(builder, environment);

#if NET6_0
            var startup = new Startup();
            
            builder.SetupWebApplicationBuilder(b => startup.ConfigureServices(b.Services));
            
            builder.SetupWebApplication(a => startup.Configure(a));
#endif
        }
    }
}