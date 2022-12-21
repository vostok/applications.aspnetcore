using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Clusterclient.Core;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

#if !ASPNTCORE_HOSTING
using Vostok.Applications.AspNetCore.Tests.Applications;
#else
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;
#endif

namespace Vostok.Applications.AspNetCore.Tests.TestHelpers
{
#if !ASPNTCORE_HOSTING
    [TestFixture(false)]
#if NET6_0_OR_GREATER
    [TestFixture(true)]
#endif
#else
    [TestFixture]
#endif
    public abstract class MiddlewareTestsBase
    {
        private int port;
        private readonly bool webApplication;
        private ITestHostRunner runner;

        protected MiddlewareTestsBase()
        {
        }

        protected MiddlewareTestsBase(bool webApplication)
        {
            this.webApplication = webApplication;
        }

        [SetUp]
        public async Task SetUp()
        {
            Log = new CompositeLog(
                new SynchronousConsoleLog(),
                new FileLog(new FileLogSettings
                {
                    FileOpenMode = FileOpenMode.Rewrite
                }));

            port = FreeTcpPortFinder.GetFreePort();
            Client = ClusterClientHelper.Create(port, Log);
            CreateRunner(SetupEnvironment);

            await runner.StartAsync();
        }

        [TearDown]
        public Task TearDown()
            => runner.StopAsync();

        protected IClusterClient Client { get; private set; }
        protected ILog Log { get; private set; }

        protected virtual void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            // use this method to override host configuration in each test fixture
        }
        
#if NET6_0_OR_GREATER
        protected virtual void SetupGlobal(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            // use this method to override host configuration in each test fixture
        }
#endif

        protected virtual void SetupGlobal(IVostokHostingEnvironmentBuilder builder)
        {
            // use this method to override vostok configuration in each test fixture
        }

#if ASPNTCORE_HOSTING
        protected virtual void SetupGlobal(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)
        {
            // use this method to override host configuration in each test fixture
        }
        
        protected virtual void SetupGlobal(Microsoft.AspNetCore.Builder.WebApplication builder)
        {
            // use this method to override host configuration in each test fixture
        }
#endif

#if !ASPNTCORE_HOSTING
        private void CreateRunner(VostokHostingEnvironmentSetup setup) =>
            runner = new TestVostokHostRunner(CreateVostokApplication(), setup);

        protected virtual IVostokApplication CreateVostokApplication()
        {
            return webApplication
#if NET6_0_OR_GREATER
                ? new TestVostokAspNetCoreWebApplication(SetupGlobal)
#else
                ? throw new Exception("Should not be called")
#endif
                : new TestVostokAspNetCoreApplication(SetupGlobal);
        }
#else
        private void CreateRunner(VostokHostingEnvironmentSetup setup) =>
            runner = new TestWebApplicationHostRunner(setup, Setup, Setup);
        
        private void Setup(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)
        {
            builder.Services.AddVostokMiddlewares();
            SetupGlobal(builder);
        }

        private void Setup(Microsoft.AspNetCore.Builder.WebApplication builder)
        {
            builder.UseVostokMiddlewares();
            SetupGlobal(builder);
        }
#endif
        
        private void SetupEnvironment(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupApplicationIdentity(
                    s => s.SetProject("Project")
                        .SetSubproject("SubProject")
                        .SetEnvironment("Env")
                        .SetApplication("App")
                        .SetInstance("Instance"))
                .SetPort(port)
                .SetupLog(s => s.AddLog(Log));

            builder.DisableClusterConfig();

            SetupGlobal(builder);
        }
    }
}