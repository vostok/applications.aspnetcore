using Vostok.Clusterclient.Core;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class ControllerTests
    {
        protected IClusterClient Client => TestEnvironment.Client;
        protected ILog Log => TestEnvironment.Log;
    }
}