using Microsoft.Extensions.Configuration;
using Vostok.Configuration.Microsoft;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class MicrosoftConfigurationBuilder
    {
        public IConfigurationSource Build(IVostokHostingEnvironment environment)
        {
            // CR(iloktionov): Надо не забыть про SecretConfigurationSource.
            return new VostokConfigurationSource(environment.ConfigurationSource);
        }
    }
}