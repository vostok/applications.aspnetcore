#if NET6_0
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.Tests;

[TestFixture]
public class MinimalApiTests : TestsBase
{
    public MinimalApiTests()
        : base(new MinimalApiApplication())
    {
    }

    [Test]
    public async Task Get_should_work()
    {
        var response = await Client.GetAsync("/");

        response.Response.IsSuccessful.Should().BeTrue();
        response.Response.Content.ToString().Should().Be("Hello World!");
    }

    internal class MinimalApiApplication : VostokAspNetCoreWebApplication
    {
        public override Task SetupAsync(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupWebApplication(app => app.MapGet("/", () => "Hello World!"));

            return Task.CompletedTask;
        }
    }
}
#endif