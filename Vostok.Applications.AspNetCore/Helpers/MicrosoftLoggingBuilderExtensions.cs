using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vostok.Configuration.Microsoft;
using Vostok.Configuration.Sources.Object;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal static class MicrosoftLoggingBuilderExtensions
    {
        public static void AddVostokLogging(this ILoggingBuilder builder, IVostokHostingEnvironment environment, VostokLoggerProviderSettings settings)
        {
            builder.ClearProviders().AddProvider(new VostokLoggerProvider(environment.Log, settings));
        }

        public static void AddDefaultLoggingFilters(this IConfigurationBuilder builder)
        {
            builder.Sources.Insert(0, new VostokConfigurationSource(new ObjectSource(new
            {
                Logging = new
                {
                    LogLevel = new
                    {
                        Default = "Information",
                        Microsoft = "Warning"
                    }
                }
            })));
        }
    }
}
