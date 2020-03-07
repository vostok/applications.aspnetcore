using Microsoft.Extensions.Configuration;
using Vostok.Configuration.Microsoft;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal static class MicrosoftConfigurationBuilderExtensions
    {
        public static void AddVostokSources(this IConfigurationBuilder builder, IVostokHostingEnvironment environment)
            => builder
                .AddVostok(environment.ConfigurationSource)
                .AddVostok(environment.SecretConfigurationSource);
    }
}
