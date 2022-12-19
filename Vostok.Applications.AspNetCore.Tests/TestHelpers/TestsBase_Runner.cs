using System;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Applications;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Applications.AspNetCore.Tests.TestHelpers
{
    [TestFixture(false)]
#if NET6_0_OR_GREATER
    [TestFixture(true)]
#endif
    public abstract partial class TestsBase
    {
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
    }
}