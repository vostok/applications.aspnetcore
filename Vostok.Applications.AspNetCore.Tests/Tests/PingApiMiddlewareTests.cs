using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture]
    public class PingApiMiddlewareTests : ControllerTests
    {
        [Test]
        public async Task Get_ShouldReturnOk_WhenReplicaInitialized()
        {
            var response = await Client.GetAsync<PingApiResponse>("/_status/ping");

            response.Status.Should().Be("Ok");
        }
    }
}