using System.Threading.Tasks;

namespace Vostok.Applications.AspNetCore.Tests.TestHelpers;

public interface ITestHostRunner
{
    Task StartAsync();
    
    Task StopAsync();
}