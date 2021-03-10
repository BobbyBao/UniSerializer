using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UniSerializer
{
    // Fast Binary Encoding dynamic bytes buffer
    public class Buffer
    {
        private byte[] _data;
        private long _size;
        private long _offset;

        // Is the buffer empty?
        public bool IsEmpty => (_data == null) || (_size == 0);
        // Bytes memory buffer
        public byte[] Data => _data;
        // Bytes memory buffer capacity
        public long Capacity => _data.Length;
        // Bytes memory buffer size
        public long Size => _size;
        // Bytes memory buffer offset
        public long Offset => _offset;

        // Initialize a new expandable buffer with zero capacity
        public Buffer() { Attach(); }
        // Initialize a new expandable buffer with the given capacity
        public Buffer(long capacity) { Attach(capacity); }
        // Initialize a new buffer based on the specified byte array
        public Buffer(byte[] buffer) { Attach(buffer); }
        // Initialize a new buffer based on the specified region (offset) of a byte array
        public Buffer(byte[] buffer, long offset) { Attach(buffer, offset); }
        // Initialize a new buffer based on the specified region (size and offset) of a byte array
        public Buffer(byte[] buffer, long size, long offset) { Attach(buffer, size, offset); }

        #region Attach memory buffer methods

        public void Attach() { _data = new byte[0]; _size = 0; _offset = 0; }
        public void Attach(long capacity) { _data = new byte[capacity]; _size = 0; _offset = 0; }
        public void Attach(byte[] buffer) { _data = buffer; _size = buffer.Length; _offset = 0; }
        public void Attach(byte[] buffer, long offset) { _data = buffer; _size = buffer.Length; _offset = offset; }
        public void Attach(byte[] buffer, long size, long offset) { _data = buffer; _size = size; _offset = offset; }

        #endregion

        #region Memory buffer methods

        // Allocate memory in the current buffer and return offset to the allocated memory block
        public long Allocate(long size)
        {
            Debug.Assert((size >= 0), "Invalid allocation size!");
            if (size < 0)
                throw new ArgumentException("Invalid allocation size!", nameof(size));

            long offset = Size;

            // Calculate a new buffer size
            long total = _size + size;

            if (total <= Capacity)
            {
                _size = total;
                return offset;
            }

            byte[] data = new byte[Math.Max(total, 2 * Capacity)];
            Array.Copy(_data, 0, data, 0, _size);
            _data = data;
            _size = total;
            return offset;
        }

        // Remove some memory of the given size from the current buffer
        public void Remove(long offset, long size)
        {
            Debug.Assert(((offset + size) <= Size), "Invalid offset & size!");
            if ((offset + size) > Size)
                throw new ArgumentException("Invalid offset & size!", nameof(offset));

            Array.Copy(_data, offset + size, _data, offset, _size - size - offset);
            _size -= size;
            if (_offset >= (offset + size))
                _offset -= size;
            else if (_offset >= offset)
            {
                _offset -= _offset - offset;
                if (_offset > Size)
                    _offset = Size;
            }
        }

        // Reserve memory of the given capacity in the current buffer
        public void Reserve(long capacity)
        {
            Debug.Assert((capacity >= 0), "Invalid reserve capacity!");
            if (capacity < 0)
                throw new ArgumentException("Invalid reserve capacity!", nameof(capacity));

            if (capacity > Capacity)
            {
                byte[] data = new byte[Math.Max(capacity, 2 * Capacity)];
                Array.Copy(_data, 0, data, 0, _size);
                _data = data;
            }
        }

        // Resize the current buffer
        public void Resize(long size)
        {
            Reserve(size);
            _size = size;
            if (_offset > _size)
                _offset = _size;
        }

        // Reset the current buffer and its offset
        public void Reset()
        {
            _size = 0;
            _offset = 0;
        }

        // Shift the current buffer offset
        public void Shift(long offset) { _offset += offset; }
        // Unshift the current buffer offset
        public void Unshift(long offset) { _offset -= offset; }

        #endregion

        #region Buffer I/O methods

        public static bool ReadBool(byte[] buffer, long offset)
        {
            return buffer[offset] != 0;
        }

        public static byte ReadByte(byte[] buffer, long offset)
        {
            return buffer[offset];
        }

        public static char ReadChar(byte[] buffer, long offset)
        {
            return (char)ReadUInt8(buffer, offset);
        }

        public static char ReadWChar(byte[] buffer, long offset)
        {
            return (char)ReadUInt32(buffer, offset);
        }

        public static sbyte ReadInt8(byte[] buffer, long offset)
        {
            return (sbyte)buffer[offset];
        }

        public static byte ReadUInt8(byte[] buffer, long offset)
        {
            return buffer[offset];
        }

        public static short ReadInt16(byte[] buffer, long offset)
        {
            return (short)(buffer[offset + 0] | (buffer[offset + 1] << 8));
        }

        public static ushort ReadUInt16(byte[] buffer, long offset)
        {
            return (ushort)(buffer[offset + 0] | (buffer[offset + 1] << 8));
        }

        public static int ReadInt32(byte[] buffer, long offset)
        {
            return (buffer[offset + 0] << 0) |
                   (buffer[offset + 1] << 8) |
                   (buffer[offset + 2] << 16) |
                   (buffer[offset + 3] << 24);
        }

        public static uint ReadUInt32(byte[] buffer, long offset)
        {
            return ((uint)buffer[offset + 0] << 0) |
                   ((uint)buffer[offset + 1] << 8) |
                   ((uint)buffer[offset + 2] << 16) |
                   ((uint)buffer[offset + 3] << 24);
        }

        public static long ReadInt64(byte[] buffer, long offset)
        {
            return ((long)buffer[offset + 0] << 0) |
                   ((long)buffer[offset + 1] << 8) |
                   ((long)buffer[offset + 2] << 16) |
                   ((long)buffer[offset + 3] << 24) |
                   ((long)buffer[offset + 4] << 32) |
                   ((long)buffer[offset + 5] << 40) |
                   ((long)buffer[offset + 6] << 48) |
                   ((long)buffer[offset + 7] << 56);
        }

        public static ulong ReadUInt64(byte[] buffer, long offset)
        {
            return ((ulong)buffer[offset + 0] << 0) |
                   ((ulong)buffer[offset + 1] << 8) |
                   ((ulong)buffer[offset + 2] << 16) |
                   ((ulong)buffer[offset + 3] << 24) |
                   ((ulong)buffer[offset + 4] << 32) |
                   ((ulong)buffer[offset + 5] << 40) |
                   ((ulong)buffer[offset + 6] << 48) |
                   ((ulong)buffer[offset + 7] << 56);
        }

        public static ulong ReadUInt64Guid(byte[] buffer, long offset)
        {
            return ((ulong)buffer[offset + 0] << 24) |
                   ((ulong)buffer[offset + 1] << 16) |
                   ((ulong)buffer[offset + 2] << 8) |
                   ((ulong)buffer[offset + 3] << 0) |
                   ((ulong)buffer[offset + 4] << 40) |
                   ((ulong)buffer[offset + 5] << 32) |
                   ((ulong)buffer[offset + 6] << 56) |
                   ((ulong)buffer[offset + 7] << 48);
        }

        public static float ReadFloat(byte[] buffer, long offset)
        {
            var bits = default(FloatUnion);
            bits.UIntData = ReadUInt32(buffer, offset);
            return bits.FloatData;
        }

        public static double ReadDouble(byte[] buffer, long offset)
        {
            var bits = default(DoubleUnion);
            bits.ULongData = ReadUInt64(buffer, offset);
            return bits.DoubleData;
        }

        public static decimal ReadDecimal(byte[] buffer, long offset)
        {
            var bits = default(DecimalUnion);
            bits.UIntLow = ReadUInt32(buffer, offset);
            bits.UIntMid = ReadUInt32(buffer, offset + 4);
            bits.UIntHigh = ReadUInt32(buffer, offset + 8);
            bits.UIntFlags = ReadUInt32(buffer, offset + 12);
            return bits.DecimalData;
        }

        public static byte[] ReadBytes(byte[] buffer, long offset, long size)
        {
            byte[] result = new byte[size];
            Array.Copy(buffer, offset, result, 0, (int)size);
            return result;
        }

        public static string ReadString(byte[] buffer, long offset, long size)
        {
            return Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        }

        public static Guid ReadUUID(byte[] buffer, long offset)
        {
            var bits = default(GuidUnion);
            bits.ULongHigh = ReadUInt64Guid(buffer, offset);
            bits.ULongLow = ReadUInt64(buffer, offset + 8);
            return bits.GuidData;
        }

        public static void Write(byte[] buffer, long offset, bool value)
        {
            buffer[offset] = (byte)(value ? 1 : 0);
        }

        public static void Write(byte[] buffer, long offset, sbyte value)
        {
            buffer[offset] = (byte)value;
        }

        public static void Write(byte[] buffer, long offset, byte value)
        {
            buffer[offset] = value;
        }

        public static void Write(byte[] buffer, long offset, short value)
        {
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
        }

        public static void Write(byte[] buffer, long offset, ushort value)
        {
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
        }

        public static void Write(byte[] buffer, long offset, int value)
        {
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        public static void Write(byte[] buffer, long offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        public static void Write(byte[] buffer, long offset, long value)
        {
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
            buffer[offset + 4] = (byte)(value >> 32);
            buffer[offset + 5] = (byte)(value >> 40);
            buffer[offset + 6] = (byte)(value >> 48);
            buffer[offset + 7] = (byte)(value >> 56);
        }

        public static void Write(byte[] buffer, long offset, ulong value)
        {
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
            buffer[offset + 4] = (byte)(value >> 32);
            buffer[offset + 5] = (byte)(value >> 40);
            buffer[offset + 6] = (byte)(value >> 48);
            buffer[offset + 7] = (byte)(value >> 56);
        }

        public static void WriteGuid(byte[] buffer, long offset, ulong value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
            buffer[offset + 4] = (byte)(value >> 40);
            buffer[offset + 5] = (byte)(value >> 32);
            buffer[offset + 6] = (byte)(value >> 56);
            buffer[offset + 7] = (byte)(value >> 48);
        }

        public static void Write(byte[] buffer, long offset, float value)
        {
            var bits = default(FloatUnion);
            bits.FloatData = value;
            Write(buffer, offset, bits.UIntData);
        }

        public static void Write(byte[] buffer, long offset, double value)
        {
            var bits = default(DoubleUnion);
            bits.DoubleData = value;
            Write(buffer, offset, bits.ULongData);
        }

        public static void Write(byte[] buffer, long offset, decimal value)
        {
            var bits = default(DecimalUnion);
            bits.DecimalData = value;
            Write(buffer, offset, bits.UIntLow);
            Write(buffer, offset + 4, bits.UIntMid);
            Write(buffer, offset + 8, bits.UIntHigh);
            Write(buffer, offset + 12, bits.UIntFlags);
        }

        public static void Write(byte[] buffer, long offset, byte[] value)
        {
            Array.Copy(value, 0, buffer, offset, value.Length);
        }

        public static void Write(byte[] buffer, long offset, byte[] value, long valueOffset, long valueSize)
        {
            Array.Copy(value, valueOffset, buffer, offset, valueSize);
        }

        public static void Write(byte[] buffer, long offset, byte value, long valueCount)
        {
            for (long i = 0; i < valueCount; i++)
                buffer[offset + i] = value;
        }

        public static long Write(byte[] buffer, long offset, string value)
        {
            return Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, (int)offset);
        }

        public static void Write(byte[] buffer, long offset, Guid value)
        {
            var bits = default(GuidUnion);
            bits.GuidData = value;
            WriteGuid(buffer, offset, bits.ULongHigh);
            Write(buffer, offset + 8, bits.ULongLow);
        }

        #endregion

        #region Utilities
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct FloatUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public uint UIntData;
            [System.Runtime.InteropServices.FieldOffset(0)]
            public float FloatData;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct DoubleUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public ulong ULongData;
            [System.Runtime.InteropServices.FieldOffset(0)]
            public double DoubleData;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct DecimalUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public uint UIntFlags;
            [System.Runtime.InteropServices.FieldOffset(4)]
            public uint UIntHigh;
            [System.Runtime.InteropServices.FieldOffset(8)]
            public uint UIntLow;
            [System.Runtime.InteropServices.FieldOffset(12)]
            public uint UIntMid;
            [System.Runtime.InteropServices.FieldOffset(0)]
            public decimal DecimalData;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct GuidUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public ulong ULongHigh;
            [System.Runtime.InteropServices.FieldOffset(8)]
            public ulong ULongLow;
            [System.Runtime.InteropServices.FieldOffset(0)]
            public Guid GuidData;
        }

        #endregion
    }

}
