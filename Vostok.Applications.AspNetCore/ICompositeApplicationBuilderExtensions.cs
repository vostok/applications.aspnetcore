using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Composite;

namespace Vostok.Applications.AspNetCore
{
    [PublicAPI]
    public static class ICompositeApplicationBuilderExtensions
    {
#if !NET6_0
        [NotNull]
        public static ICompositeApplicationBuilder AddAspNetCore<TStartup>(
            [NotNull] this ICompositeApplicationBuilder builder,
            [NotNull] Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> setup)
            where TStartup : class => builder.AddApplication(new AdHocAspnetcoreApplication<TStartup>(setup, null));

        [NotNull]
        public static ICompositeApplicationBuilder AddAspNetCore<TStartup>(
            [NotNull] this ICompositeApplicationBuilder builder,
            [NotNull] Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> setup,
            [NotNull] Func<IVostokHostingEnvironment, IServiceProvider, Task> warmup)
            where TStartup : class => builder.AddApplication(new AdHocAspnetcoreApplication<TStartup>(setup, warmup));
#endif

        [NotNull]
        public static ICompositeApplicationBuilder AddAspNetCore(
            [NotNull] this ICompositeApplicationBuilder builder,
            [NotNull] Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> setup)
            => builder.AddApplication(new AdHocAspnetcoreApplication(setup, null));

        [NotNull]
        public static ICompositeApplicationBuilder AddAspNetCore(
            [NotNull] this ICompositeApplicationBuilder builder,
            [NotNull] Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> setup,
            [NotNull] Func<IVostokHostingEnvironment, IServiceProvider, Task> warmup)
            => builder.AddApplication(new AdHocAspnetcoreApplication(setup, warmup));

#if NET6_0
        private class AdHocAspnetcoreApplication : VostokAspNetCoreApplication
#else
        private class AdHocAspnetcoreApplication<TStartup> : VostokAspNetCoreApplication<TStartup>
            where TStartup : class
#endif
        {
            private readonly Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> setup;
            private readonly Func<IVostokHostingEnvironment, IServiceProvider, Task> warmup;

            public AdHocAspnetcoreApplication(
                [NotNull] Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> setup, 
                [CanBeNull] Func<IVostokHostingEnvironment, IServiceProvider, Task> warmup)
            {
                this.setup = setup ?? throw new ArgumentNullException(nameof(setup));
                this.warmup = warmup ?? ((_, __) => Task.CompletedTask);
            }

            public override void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
                => setup(builder, environment);

            public override Task WarmupAsync(IVostokHostingEnvironment environment, IServiceProvider serviceProvider)
                => warmup(environment, serviceProvider);
        }

#if !NET6_0
        private class AdHocAspnetcoreApplication : AdHocAspnetcoreApplication<EmptyStartup>
        {
            public AdHocAspnetcoreApplication(
                [NotNull] Action<IVostokAspNetCoreApplicationBuilder, IVostokHostingEnvironment> setup, 
                [CanBeNull] Func<IVostokHostingEnvironment, IServiceProvider, Task> warmup)
                : base(setup, warmup)
            {
            }
        }
#endif
    }
}
