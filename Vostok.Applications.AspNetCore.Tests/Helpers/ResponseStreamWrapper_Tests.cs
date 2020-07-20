using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Helpers;

namespace Vostok.Applications.AspNetCore.Tests.Helpers
{
    internal class ResponseStreamWrapper_Tests
    {
        private byte[] leadingGarbage;
        private byte[] trailingGarbage;
        private byte[] payload;
        private byte[] merged;

        [TestCase(100, 101)]
        [TestCase(100, 100)]
        [TestCase(100, 99)]
        [TestCase(100, 31)]
        [TestCase(100, 3)]
        [TestCase(100, 2)]
        [TestCase(100, 1)]
        public async Task Write_calls_should_produce_correct_output(int inputLength, int maxWriteSize)
        {
            GenerateData(inputLength);

            var output1 = new MemoryStream();
            var output2 = new MemoryStream();
            var output3 = new MemoryStream();

            var wrapper1 = new ResponseStreamWrapper(output1, maxWriteSize);
            var wrapper2 = new ResponseStreamWrapper(output2, maxWriteSize);
            var wrapper3 = new ResponseStreamWrapper(output3, maxWriteSize);

            wrapper1.Write(merged, leadingGarbage.Length, payload.Length);

            await wrapper2.WriteAsync(merged, leadingGarbage.Length, payload.Length, CancellationToken.None);

            #if NETCOREAPP3_1
            await wrapper3.WriteAsync(new ReadOnlyMemory<byte>(merged, leadingGarbage.Length, payload.Length));
            #endif

            output1.ToArray().Should().Equal(payload);
            output2.ToArray().Should().Equal(payload);

            #if NETCOREAPP3_1
            output3.ToArray().Should().Equal(payload);
            #endif
        }

        private void GenerateData(int payloadLength)
        {
            leadingGarbage = RandomBytes(123);
            trailingGarbage = RandomBytes(321);
            payload = RandomBytes(payloadLength);
            merged = leadingGarbage.Concat(payload).Concat(trailingGarbage).ToArray();
        }

        private static byte[] RandomBytes(int length)
        {
            var result = new byte[length];

            new Random(Guid.NewGuid().GetHashCode()).NextBytes(result);

            return result;
        }
    }
}
