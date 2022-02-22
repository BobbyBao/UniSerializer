using MessagePack;
using MessagePack.Internal;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using static System.Buffers.BuffersExtensions;

namespace UniSerializer
{
    public class MessagePackDeserializer : Deserializer
    {
        private const int MaxHintSize = 1024 * 1024;
        static readonly byte[] typeCode = Encoding.UTF8.GetBytes("$type");
        static readonly byte[] idCode = Encoding.UTF8.GetBytes("$id");
        static readonly byte[] refCode = Encoding.UTF8.GetBytes("$ref|");

        MessagePackReader reader;
        public override T Load<T>(Stream stream)
        {
            using (var sequenceRental = SequencePool.Shared.Rent())
            {
                var sequence = sequenceRental.Value;
                try
                {
                    int bytesRead;
                    do
                    {
                        Span<byte> span = sequence.GetSpan(stream.CanSeek ? (int)Math.Min(MaxHintSize, stream.Length - stream.Position) : 0);
                        bytesRead = stream.Read(span);
                        sequence.Advance(bytesRead);
                    }
                    while (bytesRead > 0);

                    reader = new MessagePackReader(sequence);

                    T obj = default;
                    Serialize(ref obj, 1);

                    if (stream.CanSeek && !reader.End)
                    {
                        // Reverse the stream as many bytes as we left unread.
                        int bytesNotRead = checked((int)reader.Sequence.Slice(reader.Position).Length);
                        stream.Seek(-bytesNotRead, SeekOrigin.Current);
                    }

                    return obj;
                }
                catch (Exception ex)
                {
                    throw new MessagePackSerializationException("Error occurred while reading from the stream.", ex);
                }
            }

        }

        private bool TryGetProperty(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return reader.TryReadPropertyKey(bytes);
        }

        protected override bool CreateObject(out object obj)
        {
            if (reader.NextMessagePackType == MessagePackType.String)
            {
                if (reader.TryReadStringSpan(out ReadOnlySpan<byte> span))
                {
                    if (span.StartsWith(refCode))
                    {
                        if (Utf8Parser.TryParse(span.Slice(5), out int refID, out int bytesConsumed))
                        {
                            obj = Session.GetRefObject(refID);
                        }
                        else
                        {
                            obj = null;
                            Log.Error("Error refid : ", span.ToString());
                        }
                        return false;
                    }
                    else if(span.Length == 36)
                    {

                        if (Utf8Parser.TryParse(span, out Guid refID, out int bytesConsumed))
                        {
                            obj = Session.GetRefObject(refID);
                        }
                        else
                        {
                            obj = null;
                            Log.Error("Error refid : ", span.ToString());
                        }
                        return false;
                    }
                    else
                    {
                        obj = null;
                        return true;
                    }
                }

            }
            
            if (reader.NextMessagePackType != MessagePackType.Map)
            {
                obj = null;
                return true;
            }

            if (!reader.TryReadMapHeader(out int count))
            {
                obj = null;
                return true;
            }

            if (!reader.TryReadPropertyKey(typeCode))
            {
                obj = null;
                return true;
            }

            var typeName = reader.ReadString();

            var type = TypeUtilities.GetType(typeName);
            if (type == null)
            {
                obj = null;
                return true;
            }

            obj = Activator.CreateInstance(type);

            if (!reader.TryReadPropertyKey(idCode))
            {
                Log.Warning("object no id.");
                return true;
            }

            var id = reader.ReadInt32();
            var id1 = Session.AddRefObject(obj);
            System.Diagnostics.Debug.Assert(id == id1);
            return true;
        }

        public override bool StartObject<T>(ref T obj)
        {
            if (reader.TryReadNil())
            {
                obj = default;
                return false;
            }

            if (obj.GetType().IsValueType)
            {
                if (!reader.TryReadMapHeader(out var len))
                {
                    System.Diagnostics.Debug.Assert(false);
                    return false;
                }

            }

            return true;
        }

        public override void EndObject()
        {
        }

        public override bool StartProperty(string name)
        {
            var propName = reader.ReadString();
            System.Diagnostics.Debug.Assert(name == propName);
            return true;
        }

        public override void EndProperty()
        {
        }

        public override bool StartArray<T>(ref T array, ref int len)
        {
            if (reader.TryReadNil())
            {
                array = default;
                return false;
            }

            if (!reader.TryReadArrayHeader(out len))
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            return true;
        }

        public override void SetElement(int index)
        {
        }

        public override void EndArray()
        {
        }

        public override void SerializeNull()
        {
            if (!reader.TryReadNil())
            {
                Log.Error("Read nil failed.");
            }
        }

        public override void SerializePrimitive<T>(ref T val)
        {
            switch (val)
            {
                case bool v:
                    Unsafe.As<T, bool>(ref val) = reader.ReadBoolean();
                    break;
                case int v:
                    Unsafe.As<T, int>(ref val) = reader.ReadInt32();
                    break;
                case uint v:
                    Unsafe.As<T, uint>(ref val) = reader.ReadUInt32();
                    break;
                case float v:
                    Unsafe.As<T, float>(ref val) = reader.ReadSingle();
                    break;
                case double v:
                    Unsafe.As<T, double>(ref val) = reader.ReadDouble();
                    break;
                case long v:
                    Unsafe.As<T, long>(ref val) = reader.ReadInt64();
                    break;
                case ulong v:
                    Unsafe.As<T, ulong>(ref val) = reader.ReadUInt64();
                    break;
                case short v:
                    Unsafe.As<T, short>(ref val) = reader.ReadInt16();
                    break;
                case ushort v:
                    Unsafe.As<T, ushort>(ref val) = reader.ReadUInt16();
                    break;
                case char v:
                    Unsafe.As<T, char>(ref val) = reader.ReadChar();
                    break;
                case sbyte v:
                    Unsafe.As<T, sbyte>(ref val) = reader.ReadSByte();
                    break;
                case byte v:
                    Unsafe.As<T, byte>(ref val) = reader.ReadByte();
                    break;
                case decimal v:
                    //Unsafe.As<T, decimal>(ref val) = reader.ReadDecimal();
                    break;
            }

        }

        public override void SerializeString(ref string val)
        {
            if (reader.TryReadNil())
            {
                val = null;
                return;
            }

            val = reader.ReadString();
        }

        public override void SerializeBytes(ref byte[] val)
        {
            val = reader.ReadBytes()?.ToArray();
        }

        public override void SerializeGuid(ref Guid val)
        {
            if (reader.TryReadNil())
            {
                val = Guid.Empty;
                return;
            }

            System.Buffers.ReadOnlySequence<byte> segment = reader.ReadStringSequence().Value;
            if (segment.Length != 36)
            {
                throw new MessagePackSerializationException("Unexpected length of string.");
            }

            GuidBits result;
            if (segment.IsSingleSegment)
            {
                result = new GuidBits(segment.First.Span);
            }
            else
            {
                Span<byte> bytes = stackalloc byte[36];
                segment.CopyTo(bytes);
                result = new GuidBits(bytes);
            }

            val = result.Value;

        }

        public override void SerializeUnmanaged<T>(ref T val, int count)
        {
            if (!reader.TryReadArrayHeader(out var len))
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            System.Diagnostics.Debug.Assert(len == count);

            for (int i = 0; i < count; i++)
            {
                SerializePrimitive(ref Unsafe.Add(ref val, i));
            }
        }

        public override void SerializeMemory(ref IntPtr data, ref ulong length)
        {
            var seq = reader.ReadBytes();

            if(seq.HasValue)
            {
                data = Marshal.AllocHGlobal((int)seq.Value.Length);
                unsafe
                {
                    seq.Value.CopyTo(new Span<byte>((void*)data, (int)seq.Value.Length));
                }
            }
            else
            {
                data = IntPtr.Zero;
                length = 0;
            }
           
        }
    }
}
