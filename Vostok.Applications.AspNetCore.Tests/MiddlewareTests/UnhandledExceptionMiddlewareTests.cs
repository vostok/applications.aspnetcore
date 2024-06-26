using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Hosting.Abstractions;
#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Http;
#endif

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests
{
    public class UnhandledExceptionMiddlewareTests : MiddlewareTestsBase
    {
        private const int ResponseCode = 418;
        private const int CanceledResponseCode = (int)Clusterclient.Core.Model.ResponseCode.Canceled;

        public UnhandledExceptionMiddlewareTests(bool webApplication)
            : base(webApplication)
        {
        }

        public UnhandledExceptionMiddlewareTests()
        {
        }

        [Test]
        public async Task Invoke_ShouldCatch_UnhandledExceptions()
        {
            var result = await Client.GetAsync("exception");

            result.Response.Code.Should().Be(ResponseCode);
        }

#if NET5_0_OR_GREATER
        [Test]
        public async Task Invoke_ShouldNotCatch_IgnoredUnhandledExceptions()
        {
            var result = await Client.GetAsync("canceled-bad-http-exception");

            result.Response.Code.Should().Be(CanceledResponseCode);
        }
#endif

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupUnhandledExceptions(SetupUnhandledExceptions);
        }

#if NET6_0_OR_GREATER
        protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupUnhandledExceptions(SetupUnhandledExceptions);
        }
#endif

#if ASPNTCORE_HOSTING
        protected override void SetupGlobal(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder, Vostok.Hosting.AspNetCore.Web.Configuration.IVostokMiddlewaresConfigurator middlewaresConfigurator)
        {
            middlewaresConfigurator.ConfigureUnhandledExceptions(s =>
            {
                s.ErrorResponseCode = ResponseCode;
                s.ExceptionsToIgnore.Add(typeof(BadHttpRequestException));
            });
        }
#endif

        private static void SetupUnhandledExceptions(UnhandledExceptionSettings settings)
        {
            settings.ErrorResponseCode = ResponseCode;
#if NET5_0_OR_GREATER
            settings.ExceptionsToIgnore.Add(typeof(BadHttpRequestException));
#endif
        }
    }
}