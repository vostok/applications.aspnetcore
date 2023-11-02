using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
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
    
    [ApiController]
    [Route("canceled-bad-http-exception")]
    public class BadHttpExceptionController : ControllerBase
    {
        [HttpGet]
        public void Throw()
        {
            Request.HttpContext.RequestAborted = new CancellationToken(true);
            throw new BadHttpRequestException("");
        }
    }
}