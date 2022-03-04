using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Context;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.Controllers
{
    [ApiController]
    [Route("request-info")]
    public class RequestInfoController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly ILog log;

        public RequestInfoController(ILogger<RequestInfoController> logger, ILog log)
        {
            this.logger = logger;
            this.log = log;
        }
        
        public object GetRequestInfo()
        {
            logger.LogInformation("Hello");
            log.Info("Hello");
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