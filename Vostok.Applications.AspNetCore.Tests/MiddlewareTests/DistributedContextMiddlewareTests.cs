﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Clusterclient.Core.Model;
using Vostok.Context;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests
{
    public class DistributedContextMiddlewareTests : MiddlewareTestsBase
    {
        public DistributedContextMiddlewareTests(bool webApplication)
            : base(webApplication)
        {
        }

        public DistributedContextMiddlewareTests()
        {
        }

        [Theory]
        public async Task Invoke_ShouldRestoreRequestPriority(RequestPriority requestPriority)
        {
            var req = Request.Get("/context?name=request-priority");
            var priority = await Client.SendAsync(req, RequestParameters.Empty.WithPriority(requestPriority))
                .GetResponseOrDie<RequestPriority>();

            priority.Should().Be(requestPriority);
        }

        [Test]
        public async Task Invoke_ShouldSetOrdinaryPriority_IfNonePassed()
        {
            var priority = await Client.GetAsync<RequestPriority>("/context?name=request-priority");

            priority.Should().Be(RequestPriority.Ordinary);
        }

        [Test]
        public async Task Invoke_ShouldExecuteAdditionalActions()
        {
            var customContextual = await Client.GetAsync<string>("/context?name=custom-contextual&custom-contextual=some-value");

            customContextual.Should().Be("some-value");
        }

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupDistributedContext(s => s.AdditionalActions.AddRange(CreateDistributedContextActions()));
        }

#if NET6_0_OR_GREATER
        protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupDistributedContext(s => s.AdditionalActions.AddRange(CreateDistributedContextActions()));
        }
#endif

#if ASPNTCORE_HOSTING
        protected override void SetupGlobal(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder, Vostok.Hosting.AspNetCore.Web.Configuration.IVostokMiddlewaresConfigurator middlewaresConfigurator)
        {
            middlewaresConfigurator.ConfigureDistributedContext(s => s.AdditionalActions.AddRange(CreateDistributedContextActions()));
        }
#endif
        
        private static IEnumerable<Action<HttpRequest>> CreateDistributedContextActions()
        {
            yield return r =>
            {
                if (r.Query.TryGetValue("custom-contextual", out var value))
                    FlowingContext.Properties.Set("custom-contextual", value.ToString());
            };
        }
    }
}