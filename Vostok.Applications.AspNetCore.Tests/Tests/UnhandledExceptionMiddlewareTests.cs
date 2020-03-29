using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture]
    public class UnhandledExceptionMiddlewareTests : ControllerTestBase
    {
        private const int ResponseCode = 418;

        [Test]
        public async Task Invoke_ShouldCatch_UnhandledExceptions()
        {
            var result = await Client.GetAsync("exception");

            result.Response.Code.Should().Be(ResponseCode);
        }

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder)
        {
            builder.SetupUnhandledExceptions(s => s.ErrorResponseCode = ResponseCode);
        }
    }
}