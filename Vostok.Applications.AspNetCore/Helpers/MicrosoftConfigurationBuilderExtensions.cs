using Microsoft.Extensions.Configuration;
using Vostok.Configuration.Microsoft;
using Vostok.Hosting.Abstractions;
using IConfigurationSource = Vostok.Configuration.Abstractions.IConfigurationSource;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal static class MicrosoftConfigurationBuilderExtensions
    {
        public static void AddVostokSources(this IConfigurationBuilder builder, IVostokHostingEnvironment environment)
        {
            if (environment.HostExtensions.TryGet<IConfigurationSource>("MergedConfigurationSource", out var mergedSource))
                builder.AddVostok(mergedSource);
            else
                builder
                    .AddVostok(environment.ConfigurationSource)
                    .AddVostok(environment.SecretConfigurationSource);
        }
    }
}