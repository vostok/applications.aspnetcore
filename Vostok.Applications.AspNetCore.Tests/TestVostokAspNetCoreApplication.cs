using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.TestHost;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class TestVostokAspNetCoreApplication : VostokAspNetCoreApplication<Startup>
    {
        private readonly List<Action<IVostokAspNetCoreApplicationBuilder>> configurations;

        public TestVostokAspNetCoreApplication(List<Action<IVostokAspNetCoreApplicationBuilder>> configurations)
        {
            this.configurations = configurations;
        }

        public override void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            foreach (var configuration in configurations)
                configuration(builder);

            builder.SetupWebHost(s => s.UseTestServer());
        }
    }
}