using System;
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
        }
    }
}