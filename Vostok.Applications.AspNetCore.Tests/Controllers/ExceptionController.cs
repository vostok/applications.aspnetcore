using System;
using Microsoft.AspNetCore.Mvc;
#if NET5_0_OR_GREATER
using System.Threading;
using Microsoft.AspNetCore.Http;
#endif

namespace Vostok.Applications.AspNetCore.Tests.Controllers
{
    [ApiController]
    [Route("exception")]
    public class ExceptionController : ControllerBase
    {
        [HttpGet]
        public void Throw() => throw new NotImplementedException();
        
#if NET5_0_OR_GREATER
        [HttpGet("/canceled-bad-http-exception")]
        public void ThrowBadRequestException()
        {
            Request.HttpContext.RequestAborted = new CancellationToken(true);
            throw new BadHttpRequestException("");
        }
#endif
    }
}