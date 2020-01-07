using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    // TODO(iloktionov): Add extensions for customization of throttling provider
    [PublicAPI]
    public class ThrottlingSettings
    {
        public int RejectionResponseCode { get; set; } = 429;
    }
}
