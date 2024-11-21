using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests
{
    public class TracingMiddlewareWithExceptionHandlerMiddlewareTests : MiddlewareTestsBase
    {
        private readonly StubSpanSender spanSender = new();

        public TracingMiddlewareWithExceptionHandlerMiddlewareTests(bool webApplication)
            : base(webApplication)
        {
        }

        public TracingMiddlewareWithExceptionHandlerMiddlewareTests()
        {
        }

        [Test]
        public async Task TracingMiddleware_ShouldAddTraceId_WhenExceptionMiddlewareProducedResponse()
        {
            var response = (await Client.GetAsync("/")).Response;

            response.Code.Should().Be(ResponseCode.InternalServerError);
            response.Headers["Trace-Id"].Should().NotBeNullOrEmpty();
        }
        
        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupTracing(s => s.ResponseTraceIdHeader = "Trace-Id");
            builder.SetupWebHost(webHostBuilder =>
                webHostBuilder.Configure(appBuilder =>
                    appBuilder.UseExceptionHandler(appBuilder2 =>
                        appBuilder2.Run(httpContext =>
                            httpContext.Response.WriteAsync("Hello from ExceptionHandlerMiddleware!")))
                        .Run(_ => throw new Exception())));
        }

#if NET6_0_OR_GREATER        
        protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupTracing(s => s.ResponseTraceIdHeader = "Trace-Id");
            builder.CustomizeWebApplication(webApp =>
                webApp
                    .UseExceptionHandler(appBuilder =>
                        appBuilder.Run(httpContext =>
                            httpContext.Response.WriteAsync("Hello from ExceptionHandlerMiddleware!")))
                    .Run(_ => throw new Exception()));
        }
#endif

        protected override void SetupGlobal(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupTracer(tracerBuilder => tracerBuilder.AddSpanSender(spanSender));
        }

        private class StubSpanSender : ISpanSender
        {
            public List<ISpan> CaughtSpans = new();

            public void Send(ISpan span) => CaughtSpans.Add(span);
        }
    }
}