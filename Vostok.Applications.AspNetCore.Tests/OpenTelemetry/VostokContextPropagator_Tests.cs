#if NET6_0_OR_GREATER
using System;
using FluentAssertions;
using NUnit.Framework;
using OpenTelemetry.Context.Propagation;
using Vostok.Context;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
using Vostok.Applications.AspNetCore.OpenTelemetry;

namespace Vostok.Applications.AspNetCore.Tests.OpenTelemetry;

internal class VostokContextPropagator_Tests
{
    private VostokContextPropagator vostokContextPropagator = null!;
    private TraceContext traceContext = null!;

    [SetUp]
    public void SetUp()
    {
        FlowingContext.Configuration.ClearDistributedGlobals();
        vostokContextPropagator = new VostokContextPropagator(new VostokContextPropagatorSettings());
        traceContext = new TraceContext(Guid.NewGuid(), Guid.NewGuid());
    }

    [Test]
    public void Should_extract_trace_data()
    { 
        FlowingContext.Globals.Use(traceContext);
        
        FlowingContext.Configuration.RegisterDistributedGlobal(VostokContextReader.DistributedGlobalName, new TraceContextSerializer());
        
        var serialized = FlowingContext.SerializeDistributedGlobals();
        
        var result = Extract(serialized);
        result.ActivityContext.TraceId.ToString().Should().Be(traceContext.TraceId.ToString("N"));
    }
    
    [TestCase("hello")]
    [TestCase("vostok.tracing.contexx")]
    public void Should_extract_trace_data_and_skip_other_fields(string stringKey)
    {
        FlowingContext.Globals.Use(42);
        FlowingContext.Globals.Use("hello");
        FlowingContext.Globals.Use(traceContext);
        
        FlowingContext.Configuration.RegisterDistributedGlobal("int", new IntSerializer());
        FlowingContext.Configuration.RegisterDistributedGlobal("string", new StringSerializer());
        FlowingContext.Configuration.RegisterDistributedGlobal(VostokContextReader.DistributedGlobalName, new TraceContextSerializer());
        
        var serialized = FlowingContext.SerializeDistributedGlobals();
        
        var result = Extract(serialized);
        result.ActivityContext.TraceId.ToString().Should().Be(traceContext.TraceId.ToString("N"));
    }

    private PropagationContext Extract(string input)
    {
        return vostokContextPropagator.Extract(
            default,
            input,
            (carrier, key) =>
            {
                return key == VostokContextPropagator.ContextGlobalsHeader
                    ? new[] {carrier}
                    : Array.Empty<string>();
            });
    }

    internal class IntSerializer : IContextSerializer<int>
    {
        public string Serialize(int value) =>
            value.ToString();

        public int Deserialize(string input) =>
            int.Parse(input);
    }
    
    internal class StringSerializer : IContextSerializer<string>
    {
        public string Serialize(string value) =>
            value;

        public string Deserialize(string input) =>
            input;
    }
}
#endif