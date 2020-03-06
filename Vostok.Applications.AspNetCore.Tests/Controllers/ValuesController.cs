using Microsoft.AspNetCore.Mvc;

namespace Vostok.Applications.AspNetCore.Tests.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        public string GetValue() => "Hello world!";
    }
}