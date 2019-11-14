using Microsoft.Extensions.Configuration;
using Vostok.Configuration.Microsoft;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class MicrosoftConfigurationBuilder
    {
        public IConfigurationSource Build(IVostokHostingEnvironment environment)
        {
            return new VostokConfigurationSource(environment.ConfigurationSource);
        }
    }
}