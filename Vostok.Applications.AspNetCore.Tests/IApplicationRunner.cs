using System.Threading.Tasks;
using Vostok.Hosting.Setup;

namespace Vostok.Applications.AspNetCore.Tests;

public interface IApplicationRunner
{
    Task RunAsync();
    
    Task StopAsync();
}