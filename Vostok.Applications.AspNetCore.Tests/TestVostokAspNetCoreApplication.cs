using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Context;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class TestVostokAspNetCoreApplication : VostokAspNetCoreApplication<Startup>
    {
        public override void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupDistributedContext(c => c.AdditionalActions.AddRange(CreateDistributedContextActions()));
        }

        private IEnumerable<Action<HttpRequest>> CreateDistributedContextActions()
        {
            yield return r =>
            {
                if (r.Query.TryGetValue("custom-contextual", out var value))
                    FlowingContext.Properties.Set("custom-contextual", value.ToString());
            };
        }
    }
}