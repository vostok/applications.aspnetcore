using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore.Tests.Models;

public class ThrottlingInfoResponse
{
    public ThrottlingInfo CurrentInfo { get; set; }
    public int RejectionResponseCode { get; set; }
    public bool AddMethodProperty { get; set; }
}