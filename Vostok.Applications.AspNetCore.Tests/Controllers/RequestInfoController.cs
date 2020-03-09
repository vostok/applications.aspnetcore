using Microsoft.AspNetCore.Mvc;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Context;

namespace Vostok.Applications.AspNetCore.Tests.Controllers
{
    [ApiController]
    [Route("request-info")]
    public class RequestInfoController : ControllerBase
    {
        public object GetRequestInfo()
        {
            var requestInfo = FlowingContext.Globals.Get<IRequestInfo>();

            return new RequestInfoResponse
            {
                Priority = requestInfo.Priority,
                ClientApplicationIdentity = requestInfo.ClientApplicationIdentity,
                Timeout = requestInfo.Timeout
            };
        }
    }
}