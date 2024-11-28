using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.OpenTelemetry;

namespace Vostok.Applications.AspNetCore.Tests.OpenTelemetry;

internal class TraceParentHeaderHelper_Test
{
    [Test]
    public void Should_parse()
    {
        const string traceparent = "00-2db15f2b91d0f2238a6ca0ddfc39b1fe-b7fa253c6698fd10-01";

        TraceParentHeaderHelper.TryParse(traceparent, out var traceId, out var spanId).Should().BeTrue();

        traceId.Should().Be(Guid.Parse("2db15f2b91d0f2238a6ca0ddfc39b1fe"));
        spanId.Should().Be(Guid.Parse("b7fa253c6698fd100000000000000000"));
    }

    [TestCase("0002db15f2b91d0f2238a6ca0ddfc39b1fe0b7fa253c6698fd10001")]
    [TestCase("002db15f2b91d0f2238a6ca0ddfc39b1feb7fa253c6698fd1001")]
    [TestCase("00-2db15f2b91d0f2238a6ca0ddfc39b1fe-b7fa253c6698fd10")]
    [TestCase("00-2db15f2b91d0f2238a6ca0ddfc39b1fe-b7fa253c6698fd10-02462612")]
    [TestCase("lkfgh543hg89h35948gh5g")]
    [TestCase("00-2zz15f2b91d0f2238a6ca0ddfc39b1fe-b7fa253c6698fd10-01")]
    [TestCase("00-2db15f2b91d0f2238a6ca0ddfc39b1fe-yyya253c6698fd10-01")]
    [TestCase("00-2db15f2b91d0f2238a6ca0ddfc39b1fe-b7fa253c6623423423423498fd10-01")]
    [TestCase("00-215123531252435235db15f2b91d0f2238a6ca0ddfc39b1fe-0000000000000000-01")]
    [TestCase("01-2db15f2b91d0f2238a6ca0ddfc39b1fe-0000000000000000-01")]
    [TestCase("e0-2db15f2b91d0f2238a6ca0ddfc39b1fe-0000000000000000-01")]
    [TestCase("11-2db15f2b91d0f2238a6ca0ddfc39b1fe-0000000000000000-01")]
    [TestCase("00-00000000000000000000000000000000-b7fa253c6698fd10-01")]
    [TestCase("00-00000000000000000000000000000000-0000000000000000-01")]
    public void Should_not_parse_wrong_strings(string traceParent)
    {
        TraceParentHeaderHelper.TryParse(traceParent, out _, out _).Should().BeFalse();
    }
}