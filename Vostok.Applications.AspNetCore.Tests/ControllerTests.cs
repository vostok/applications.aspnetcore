using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class ControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            Factory = new WebApplicationFactory();
        }

        [TearDown]
        public void TearDown()
        {
            Factory?.Dispose();
        }

        protected WebApplicationFactory<Startup> Factory { get; private set; }

        protected HttpClient Client => Factory.CreateClient();
    }
}