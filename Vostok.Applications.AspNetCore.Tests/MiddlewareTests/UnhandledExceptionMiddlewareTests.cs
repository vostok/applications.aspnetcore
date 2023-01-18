using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests
{
    public class UnhandledExceptionMiddlewareTests : MiddlewareTestsBase
    {
        private const int ResponseCode = 418;

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

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupUnhandledExceptions(s => s.ErrorResponseCode = ResponseCode);
        }

#if NET6_0_OR_GREATER
        protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupUnhandledExceptions(s => s.ErrorResponseCode = ResponseCode);
        }
#endif
        
#if ASPNTCORE_HOSTING
        protected override void SetupGlobal(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder, Vostok.Hosting.AspNetCore.Web.Configuration.IVostokMiddlewaresConfigurator middlewaresConfigurator)
        {
            middlewaresConfigurator.ConfigureUnhandledExceptions(s => s.ErrorResponseCode = ResponseCode);
        }
#endif
    }
}