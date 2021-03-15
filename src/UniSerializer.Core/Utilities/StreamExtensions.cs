using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniSerializer
{
    public static class StreamExtensions
    {


#if !NETCOREAPP
        static async ValueTask<int> FinishReadAsync(Task<int> readTask, byte[] localBuffer, Memory<byte> localDestination)
        {
            try
            {
                int result = await readTask.ConfigureAwait(false);
                new ReadOnlySpan<byte>(localBuffer, 0, result).CopyTo(localDestination.Span);
                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(localBuffer);
            }
        }

        public static ValueTask<int> ReadAsync(this Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (System.Runtime.InteropServices.MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                return new ValueTask<int>(stream.ReadAsync(array.Array, array.Offset, array.Count, cancellationToken));
            }

            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            return FinishReadAsync(stream.ReadAsync(sharedBuffer, 0, buffer.Length, cancellationToken), sharedBuffer, buffer);

        }


        public static int Read(this Stream stream, Span<byte> buffer)
        {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int numRead = stream.Read(sharedBuffer, 0, buffer.Length);
                if ((uint)numRead > (uint)buffer.Length)
                {
                    throw new IOException("SR.IO_StreamTooLong");
                }

                new ReadOnlySpan<byte>(sharedBuffer, 0, numRead).CopyTo(buffer);
                return numRead;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }

        public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
        {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(sharedBuffer);
                stream.Write(sharedBuffer, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }


#endif

    }
}
