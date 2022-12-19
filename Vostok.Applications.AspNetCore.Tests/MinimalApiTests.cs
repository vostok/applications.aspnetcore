#if NET6_0_OR_GREATER
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests;

[TestFixture]
public class MinimalApiTests : TestsBase
{
    [Test]
    public async Task Get_should_work()
    {
        var response = await Client.GetAsync("/");

        response.Response.IsSuccessful.Should().BeTrue();
        response.Response.Content.ToString().Should().Be("Hello World!");
    }

    protected override IVostokApplication CreateVostokApplication()
        => new MinimalApiApplication();
    
    internal class MinimalApiApplication : VostokAspNetCoreWebApplication
    {
        public override Task SetupAsync(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.CustomizeWebApplication(app => app.MapGet("/", () => "Hello World!"));

            return Task.CompletedTask;
        }
    }
}
#endif