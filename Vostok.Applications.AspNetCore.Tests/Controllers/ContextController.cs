using Microsoft.AspNetCore.Mvc;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Context;

namespace Vostok.Applications.AspNetCore.Tests.Controllers
{
    [ApiController]
    [Route("/context")]
    public class ContextController : ControllerBase
    {
        [Produces("application/json")]
        public object GetContextualVariable(string name) =>
            name switch
            {
                "request-priority" => FlowingContext.Globals.Get<IRequestInfo>().Priority,
                _ => FlowingContext.Properties.Get<string>(name)
            };
    }
}