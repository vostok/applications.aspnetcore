using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.Setup;

namespace Vostok.Applications.AspNetCore.Tests.MiddlewareTests;

public class UsePathBaseMiddlewareTests : MiddlewareTestsBase
{
    public UsePathBaseMiddlewareTests(bool webApplication)
        : base(webApplication)
    {
    }

    public UsePathBaseMiddlewareTests()
    {
    }

    [Test]
    public async Task Should_cut_url()
    {
        var request = Request.Get("hello/request-info");

        var response = await Client.SendAsync(request, timeout: TimeSpan.FromSeconds(20))
            .GetResponseOrDie<RequestInfoResponse>();
    }

    protected override void SetupGlobal(IVostokHostingEnvironmentBuilder builder)
    {
        builder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(info => info.SetUrlPath("hello")));
    }
}