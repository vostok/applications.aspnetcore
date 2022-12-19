﻿using System.Threading.Tasks;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

namespace Vostok.Applications.AspNetCore.Tests.TestHelpers
{
    public abstract partial class MiddlewareTestsBase
    {
        protected int Port;
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

            Port = FreeTcpPortFinder.GetFreePort();
            Client = ClusterClientHelper.Create(Port, Log);
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

        private void SetupEnvironment(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupApplicationIdentity(
                    s => s.SetProject("Project")
                        .SetSubproject("SubProject")
                        .SetEnvironment("Env")
                        .SetApplication("App")
                        .SetInstance("Instance"))
                .SetPort(Port)
                .SetupLog(s => s.AddLog(Log));

            builder.DisableClusterConfig();

            SetupGlobal(builder);
        }
    }
}