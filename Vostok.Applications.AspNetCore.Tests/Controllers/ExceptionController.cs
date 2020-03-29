using System;
using Microsoft.AspNetCore.Mvc;

namespace Vostok.Applications.AspNetCore.Tests.Controllers
{
    [ApiController]
    [Route("exception")]
    public class ExceptionController : ControllerBase
    {
        [HttpGet]
        public void Throw() => throw new NotImplementedException();
    }
}