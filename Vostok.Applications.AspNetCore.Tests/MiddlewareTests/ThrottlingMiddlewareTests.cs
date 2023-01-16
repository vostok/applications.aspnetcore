using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Clusterclient.Core.Model;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Sources.Object;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Throttling;
using Vostok.Throttling.Quotas;
#if ASPNTCORE_HOSTING
using Vostok.Hosting.AspNetCore.Web.Configuration;
#endif

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests;

public class ThrottlingMiddlewareTests : MiddlewareTestsBase
{
    private PropertyQuotaOptions propertyQuotaOptions = new PropertyQuotaOptions(); 
    
    public ThrottlingMiddlewareTests(bool webApplication)
        : base(webApplication)
    {
    }

    public ThrottlingMiddlewareTests()
    {
    }
    
    [Test]
    public async Task Should_be_configurable()
    {
        var request = Request.Get("throttling-info");

        var response = await Client.SendAsync(request, timeout: TimeSpan.FromSeconds(20))
            .GetResponseOrDie<ThrottlingInfoResponse>();

        response.RejectionResponseCode.Should().Be(503);
        response.AddMethodProperty.Should().BeFalse();
        response.CurrentInfo.PerPropertyConsumption[WellKnownThrottlingProperties.Url]["throttling-info"].Should().Be(1);
        response.CurrentInfo.PerPropertyConsumption.ContainsKey(WellKnownThrottlingProperties.Method).Should().BeFalse();
    }

    [Test]
    public async Task Should_throttle_by_custom_quota()
    {
        var requestBad = Request.Get("throttling-info")
            .WithHeader("custom", "bad");
        var requestGood = Request.Get("throttling-info")
            .WithHeader("custom", "good");

        var responseBad = await Client.SendAsync(requestBad, timeout: TimeSpan.FromSeconds(20));
        responseBad.Response.Code.Should().Be(503);
        var responseGood = await Client.SendAsync(requestGood, timeout: TimeSpan.FromSeconds(20));
        responseGood.Response.Code.Should().Be(200);
    }

    protected override void SetupGlobal(IVostokHostingEnvironmentBuilder builder)
    {
        builder.SetupConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddSource(new ObjectSource(new
            {
                IndividualLimits = new Dictionary<string, double>
                {
                    ["bad"] = 0,
                    ["good"] = 100
                }
            }).Nest("customQuota"));
        });
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
            
            var configSource = environment.ConfigurationSource.ScopeTo("customQuota");
            var configProvider = environment.ConfigurationProvider;
            throttlingBuilder.UseCustomPropertyQuota("custom", 
                context => context.Request.Headers["custom"], 
                () => configProvider.Get<PropertyQuotaOptions>(configSource));
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

            var configSource = environment.ConfigurationSource.ScopeTo("customQuota");
            var configProvider = environment.ConfigurationProvider;
            throttlingBuilder.UseCustomPropertyQuota("custom", 
                context => context.Request.Headers["custom"], 
                () => configProvider.Get<PropertyQuotaOptions>(configSource));
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

        var configSource = environment.ConfigurationSource.ScopeTo("customQuota");
        var configProvider = environment.ConfigurationProvider;
        throttlingBuilder.UseCustomPropertyQuota("custom", 
            context => context.Request.Headers["custom"], 
            () => configProvider.Get<PropertyQuotaOptions>(configSource));
    }
#endif
}