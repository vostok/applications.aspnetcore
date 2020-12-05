#if NETCOREAPP
using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Composite;

namespace Vostok.Applications.AspNetCore
{
    [PublicAPI]
    public static class ICompositeApplicationBuilderExtensionsCore
    {
        [NotNull]
        public static ICompositeApplicationBuilder AddNetCore(
            [NotNull] this ICompositeApplicationBuilder builder,
            [NotNull] Action<IVostokNetCoreApplicationBuilder, IVostokHostingEnvironment> setup)
            => builder.AddApplication(new AdHocNetCoreApplication(setup));

        private class AdHocNetCoreApplication : VostokNetCoreApplication
        {
            private readonly Action<IVostokNetCoreApplicationBuilder, IVostokHostingEnvironment> setup;

            public AdHocNetCoreApplication(Action<IVostokNetCoreApplicationBuilder, IVostokHostingEnvironment> setup)
                => this.setup = setup ?? throw new ArgumentNullException(nameof(setup));

            public override void Setup(IVostokNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
                => setup(builder, environment);
        }
    }
}
#endif
