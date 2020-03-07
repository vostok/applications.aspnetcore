using Microsoft.Extensions.Logging;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal static class LoggingBuilderExtensions
    {
        public static void AddVostokLogging(this ILoggingBuilder builder, IVostokHostingEnvironment environment, VostokLoggerProviderSettings settings)
            => builder.ClearProviders().AddProvider(new VostokLoggerProvider(environment.Log, settings));
    }
}
