using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.Abstractions;
using Vostok.Throttling;
using Vostok.Throttling.Quotas;
#if ASPNTCORE_HOSTING
using Vostok.Hosting.AspNetCore.Web.Configuration;
#endif

namespace Vostok.Hosting.AspNetCore.Tests;

[TestFixture]
internal class ThrottlingMiddlewareTests : MiddlewareTestsBase
{
    private PropertyQuotaOptions propertyQuotaOptions = new PropertyQuotaOptions(); 
    
    [Test]
    public async Task Should_be_configurable()
    {
        var request = Request.Get("throttling-info");

        var response = await Client.SendAsync(request, timeout: TimeSpan.FromSeconds(20))
            .GetResponseOrDie<ThrottlingInfoResponse>();

        response.Settings.RejectionResponseCode.Should().Be(503);
        response.Settings.AddMethodProperty.Should().BeFalse();
        response.CurrentInfo.PerPropertyConsumption[WellKnownThrottlingProperties.Url]["throttling-info"].Should().Be(1);
        response.CurrentInfo.PerPropertyConsumption.ContainsKey(WellKnownThrottlingProperties.Method).Should().BeFalse();
    }
    
    protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
    {
        builder.SetupThrottling(throttlingBuilder =>
        {
            throttlingBuilder.CustomizeMiddleware(middleware =>
            {
                middleware.RejectionResponseCode = 503;
                middleware.AddMethodProperty = false;
            });

            throttlingBuilder.UseUrlQuota(() => propertyQuotaOptions);
        });
    }

#if NET6_0_OR_GREATER
    protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
    {
        builder.SetupThrottling(throttlingBuilder =>
        {
            throttlingBuilder.CustomizeMiddleware(middleware =>
            {
                middleware.RejectionResponseCode = 503;
                middleware.AddMethodProperty = false;
            });

            throttlingBuilder.UseUrlQuota(() => propertyQuotaOptions);
        });
    }
#endif

#if ASPNTCORE_HOSTING
    protected override void SetupGlobal(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder, Vostok.Hosting.AspNetCore.Web.Configuration.IVostokMiddlewaresConfigurator middlewaresConfigurator)
    {
        middlewaresConfigurator.ConfigureThrottling(throttlingConfigurator =>
        {
            throttlingConfigurator.ConfigureMiddleware(middleware => middleware.RejectionResponseCode = 503);

            throttlingConfigurator.UseUrlQuota(() => propertyQuotaOptions);
        });

        builder.Services.Configure<ThrottlingSettings>(middleware =>
            middleware.AddMethodProperty = false);
    }
#endif
}