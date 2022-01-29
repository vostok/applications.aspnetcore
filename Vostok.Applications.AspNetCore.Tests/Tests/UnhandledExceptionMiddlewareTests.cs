using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture(false)]
#if NET6_0
    [TestFixture(true)]
#endif
    public class UnhandledExceptionMiddlewareTests : ControllerTestBase
    {
        private const int ResponseCode = 418;

        public UnhandledExceptionMiddlewareTests(bool webApplication)
            : base(webApplication)
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

#if NET6_0
        protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupUnhandledExceptions(s => s.ErrorResponseCode = ResponseCode);
        }
#endif
    }
}