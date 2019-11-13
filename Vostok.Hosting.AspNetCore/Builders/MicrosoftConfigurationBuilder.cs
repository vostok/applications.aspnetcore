using Microsoft.Extensions.Configuration;
using Vostok.Configuration.Microsoft;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Setup;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class MicrosoftConfigurationBuilder : IVostokMicrosoftConfigurationBuilder
    {
        public IConfigurationSource Build(IVostokHostingEnvironment environment)
        {
            return new VostokConfigurationSource(environment.ConfigurationSource);
        }
    }
}