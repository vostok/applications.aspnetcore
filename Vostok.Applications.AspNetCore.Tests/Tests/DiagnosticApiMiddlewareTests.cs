using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture(false)]
#if NET6_0
    [TestFixture(true)]
#endif
    public class DiagnosticApiMiddlewareTests : ControllerTestBase
    {
        public DiagnosticApiMiddlewareTests(bool webApplication)
            : base(webApplication)
        {
        }
        
        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupDiagnosticApi(settings => settings.ProhibitedHeaders.Add("Prohibited"));
        }

#if NET6_0
        protected override void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupDiagnosticApi(settings => settings.ProhibitedHeaders.Add("Prohibited"));
        }
#endif

        [Test]
        public async Task Root_path_should_return_an_html_page_with_a_list_of_registered_info_providers()
        {
            var response = (await Client.GetAsync("/_diagnostic")).Response.EnsureSuccessStatusCode();

            response.Headers[HeaderNames.ContentType].Should().Be("text/html");
            response.HasContent.Should().BeTrue();

            Log.Info(response.Content.ToString());
        }

        [Test]
        public async Task Root_path_should_reject_requests_with_prohibited_headers()
        {
            var request = Request.Get("/_diagnostic").WithHeader("Prohibited", "yep");

            var response = (await Client.SendAsync(request)).Response;

            response.Code.Should().Be(ResponseCode.Forbidden);
            response.HasContent.Should().BeFalse();
        }

        [Test]
        public async Task Info_path_should_return_json_info_for_registered_entries()
        {
            var response = (await Client.GetAsync($"/_diagnostic/{DiagnosticConstants.Component}/request-throttling")).Response.EnsureSuccessStatusCode();

            response.Headers[HeaderNames.ContentType].Should().Be("application/json");
            response.HasContent.Should().BeTrue();

            Log.Info(response.Content.ToString());
        }

        [Test]
        public async Task Info_path_should_reject_requests_with_prohibited_headers()
        {
            var request = Request.Get($"/_diagnostic/{DiagnosticConstants.Component}/request-throttling")
                .WithHeader("Prohibited", "yep");

            var response = (await Client.SendAsync(request)).Response;

            response.Code.Should().Be(ResponseCode.Forbidden);
            response.HasContent.Should().BeFalse();
        }

        [Test]
        public async Task Info_path_should_not_handle_requests_with_unknown_entries()
        {
            var request = Request.Get("/_diagnostic/unknown-component/request-throttling");

            var response = (await Client.SendAsync(request)).Response;

            response.Code.Should().Be(ResponseCode.NotFound);
            response.HasContent.Should().BeFalse();
        }
    }
}