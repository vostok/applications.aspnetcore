using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore.Tests.Controllers;

[ApiController]
[Route("throttling-info")]
public class ThrottlingInfoController : ControllerBase
{
    private readonly IOptions<ThrottlingSettings> options;
    private readonly ThrottlingProvider provider;

    public ThrottlingInfoController(IOptions<ThrottlingSettings> options, IThrottlingProvider provider)
    {
        this.options = options;
        this.provider = (ThrottlingProvider)provider;
    }
        
    public object GetThrottlingInfo()
    {
        return new ThrottlingInfoResponse
        {
            Settings = options.Value,
            CurrentInfo = provider.CurrentInfo
        };
    }
}