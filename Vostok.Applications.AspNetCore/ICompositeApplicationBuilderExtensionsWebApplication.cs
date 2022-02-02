#if NET6_0
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Composite;

namespace Vostok.Applications.AspNetCore
{
    [PublicAPI]
    public static class ICompositeApplicationBuilderExtensionsWebApplication
    {
        [NotNull]
        public static ICompositeApplicationBuilder AddAspNetCoreWeb(
            [NotNull] this ICompositeApplicationBuilder builder,
            [NotNull] Func<IVostokAspNetCoreWebApplicationBuilder, IVostokHostingEnvironment, Task> setup)
            => builder.AddApplication(new AdHocAspnetcoreApplication(setup, null));

        [NotNull]
        public static ICompositeApplicationBuilder AddAspNetCoreWeb(
            [NotNull] this ICompositeApplicationBuilder builder,
            [NotNull] Func<IVostokAspNetCoreWebApplicationBuilder, IVostokHostingEnvironment, Task> setup,
            [NotNull] Func<IVostokHostingEnvironment, WebApplication, Task> warmup)
            => builder.AddApplication(new AdHocAspnetcoreApplication(setup, warmup));

        private class AdHocAspnetcoreApplication : VostokAspNetCoreWebApplication
        {
            private readonly Func<IVostokAspNetCoreWebApplicationBuilder, IVostokHostingEnvironment, Task> setup;
            private readonly Func<IVostokHostingEnvironment, WebApplication, Task> warmup;

            public AdHocAspnetcoreApplication(
                [NotNull] Func<IVostokAspNetCoreWebApplicationBuilder, IVostokHostingEnvironment, Task> setup,
                [CanBeNull] Func<IVostokHostingEnvironment, WebApplication, Task> warmup)
            {
                this.setup = setup ?? throw new ArgumentNullException(nameof(setup));
                this.warmup = warmup ?? ((_, __) => Task.CompletedTask);
            }

            public override Task SetupAsync(IVostokAspNetCoreWebApplicationBuilder builder, IVostokHostingEnvironment environment)
                => setup(builder, environment);

            public override Task WarmupAsync(IVostokHostingEnvironment environment, WebApplication webApplication)
                => warmup(environment, webApplication);
        }
    }
}
#endif