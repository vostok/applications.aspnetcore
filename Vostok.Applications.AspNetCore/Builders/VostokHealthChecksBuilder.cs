#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokHealthChecksBuilder
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly VostokDisposables disposables;

        public VostokHealthChecksBuilder(IVostokHostingEnvironment environment, VostokDisposables disposables)
        {
            this.environment = environment;
            this.disposables = disposables;
        }

        public void Register(IServiceCollection services)
        {
            services.AddVostokHealthChecks(environment);
        }
    }
}
#endif