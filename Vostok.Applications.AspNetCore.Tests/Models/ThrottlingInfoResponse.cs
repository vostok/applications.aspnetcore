using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore.Tests.Models;

public class ThrottlingInfoResponse
{
    public ThrottlingSettings Settings { get; set; }
    public ThrottlingInfo CurrentInfo { get; set; }
}