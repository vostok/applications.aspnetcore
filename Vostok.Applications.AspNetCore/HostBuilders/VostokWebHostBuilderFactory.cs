using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    [PublicAPI]
    public static class VostokWebHostBuilderFactory
    {
        public static (IVostokKestrelBuilder, IVostokThrottlingBuilder, IVostokMiddlewaresBuilder, IVostokWebHostBuilder) Create<TStartup>(
            IVostokHostingEnvironment environment,
            List<IDisposable> disposables
        )
            where TStartup : class
        {
            var kestrelBuilder = new VostokKestrelBuilder();
            var throttlingBuilder = new VostokThrottlingBuilder(environment, disposables);
            var middlewaresBuilder = new VostokMiddlewaresBuilder(environment, disposables, throttlingBuilder);
            var webHostBuilder = new VostokWebHostBuilder(environment, kestrelBuilder, middlewaresBuilder, disposables, typeof(TStartup));

            return (kestrelBuilder, throttlingBuilder, middlewaresBuilder, webHostBuilder);
        }
    }
}