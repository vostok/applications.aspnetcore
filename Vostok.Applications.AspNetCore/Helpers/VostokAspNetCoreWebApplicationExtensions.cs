#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    [PublicAPI]
    public static class VostokAspNetCoreWebApplicationExtensions
    {
        public static void SetupWebApplicationBuilder(
            [NotNull] this WebApplicationBuilder webApplicationBuilder,
            VostokAspNetCoreWebApplicationSetup setup,
            IVostokHostingEnvironment environment)
        {
            var disposables = new List<IDisposable>();
            var vostokBuilder = new VostokAspNetCoreCustomizeWebApplicationBuilder(environment, new EmptyApplication(), disposables);
            vostokBuilder.SetupPingApi(PingApiSettingsSetup.Get(environment, typeof(WebApplicationBuilder), false));
            
            setup(vostokBuilder, environment);
            
            vostokBuilder.CustomizeWebApplicationBuilder(webApplicationBuilder);
        }
    }
    
    internal class EmptyApplication : VostokAspNetCoreWebApplication
    {
        public override Task SetupAsync(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment) =>
            Task.CompletedTask;
    }
}
#endif