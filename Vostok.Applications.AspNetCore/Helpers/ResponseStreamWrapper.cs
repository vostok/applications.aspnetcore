using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Applications.AspNetCore.Helpers
{
    /// <summary>
    /// Splits large Write and WriteAsync calls to response body stream into multiple to prevent excessive unintended buffering.
    /// </summary>
    internal class ResponseStreamWrapper : Stream
    {
        private readonly Stream stream;
        private readonly int maxWriteSize;

        public ResponseStreamWrapper(Stream stream, int maxWriteSize)
        {
            this.stream = stream;
            this.maxWriteSize = maxWriteSize;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count <= maxWriteSize)
                stream.Write(buffer, offset, count);
            else
            {
                while (count > 0)
                {
                    var bytesToWrite = Math.Min(maxWriteSize, count);

                    stream.Write(buffer, offset, bytesToWrite);

                    offset += bytesToWrite;
                    count -= bytesToWrite;
                }
            }
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (count <= maxWriteSize)
                return stream.WriteAsync(buffer, offset, count, cancellationToken);

            return WriteWithMultipleCallsAsync(buffer, offset, count, cancellationToken);
        }

        private async Task WriteWithMultipleCallsAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            while (count > 0)
            {
                var bytesToWrite = Math.Min(maxWriteSize, count);

                await stream.WriteAsync(buffer, offset, bytesToWrite, cancellationToken);

                offset += bytesToWrite;
                count -= bytesToWrite;
            }
        }

        #if NETCOREAPP
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
        {
            if (source.Length <= maxWriteSize)
                return stream.WriteAsync(source, cancellationToken);

            return WriteWithMultipleCallsAsync(source, cancellationToken);
        }

        private async ValueTask WriteWithMultipleCallsAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken)
        {
            while (source.Length > 0)
            {
                var bytesToWrite = Math.Min(maxWriteSize, source.Length);

                await stream.WriteAsync(source.Slice(0, bytesToWrite), cancellationToken);

                source = source.Slice(bytesToWrite);
            }
        }
        #endif

        #region Delegating members

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override bool CanTimeout => stream.CanTimeout;

        public override long Length => stream.Length;

        public override long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        public override int ReadTimeout
        {
            get => stream.ReadTimeout;
            set => stream.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => stream.WriteTimeout;
            set => stream.WriteTimeout = value;
        }

        public override void Flush()
            => stream.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken)
            => stream.FlushAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count)
            => stream.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => stream.ReadAsync(buffer, offset, count, cancellationToken);

        #if NETCOREAPP
        public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
            => stream.ReadAsync(destination, cancellationToken);
        #endif

        public override int ReadByte()
            => stream.ReadByte();

        public override long Seek(long offset, SeekOrigin origin)
            => stream.Seek(offset, origin);

        public override void SetLength(long value)
            => stream.SetLength(value);

        public override void WriteByte(byte value)
            => stream.WriteByte(value);

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => stream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => stream.BeginRead(buffer, offset, count, callback, state);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => stream.BeginWrite(buffer, offset, count, callback, state);

        public override int EndRead(IAsyncResult asyncResult)
            => stream.EndRead(asyncResult);

        public override void EndWrite(IAsyncResult asyncResult)
            => stream.EndWrite(asyncResult);

        public override object InitializeLifetimeService()
            => stream.InitializeLifetimeService();

        public override void Close()
            => stream.Close();

        public override bool Equals(object obj)
            => stream.Equals(obj);

        public override int GetHashCode()
            => stream.GetHashCode();

        public override string ToString()
            => stream.ToString();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                stream.Dispose();
        }

        #endregion
    }
}
