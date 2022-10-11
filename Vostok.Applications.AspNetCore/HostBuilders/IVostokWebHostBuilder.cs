using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    [PublicAPI]
    public interface IVostokWebHostBuilder
    {
        void Disable();
        
        void ConfigureWebHost(IWebHostBuilder hostBuilder);

        void Customize(Action<IWebHostBuilder> setup);
        
#if NETCOREAPP
        void ConfigureWebHost(IHostBuilder genericHostBuilder);
#endif        

#if NET6_0_OR_GREATER
        void ConfigureWebHost(WebApplicationBuilder builder);
#endif
    }
}