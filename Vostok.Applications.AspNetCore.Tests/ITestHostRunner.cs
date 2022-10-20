using System.Threading.Tasks;

namespace Vostok.Applications.AspNetCore.Tests;

public interface ITestHostRunner
{
    Task StartAsync();
    
    Task StopAsync();
}