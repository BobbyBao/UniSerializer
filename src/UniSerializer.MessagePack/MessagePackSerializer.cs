using MessagePack;
using MessagePack.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public class MessagePackSerializer : Serializer
    {
        public const int MAX_DEPTH = 64;
        MessagePackWriter writer;

        int depth = 0;
        uint[] propertyNum = new uint[MAX_DEPTH];
        IntPtr[] lenAddr = new IntPtr[MAX_DEPTH];

        private ref uint PropertyNum => ref propertyNum[depth - 1];

        public override void Save<T>(T obj, Stream stream)
        {
            using (SequencePool.Rental sequenceRental = SequencePool.Shared.Rent())
            {
                writer = new MessagePackWriter(sequenceRental.Value);

                try
                {
                    if (obj.GetType() != typeof(T))
                    {
                        object o = obj;
                        Serialize(ref o, 1);
                    }
                    else
                        Serialize(ref obj, 1);

                    writer.Flush();

                    foreach (ReadOnlyMemory<byte> segment in sequenceRental.Value.AsReadOnlySequence)
                    {
                        stream.Write(segment.Span);
                    }
                }
                catch (Exception ex)
                {
                    throw new MessagePackSerializationException("Error occurred while writing the serialized data to the stream.", ex);
                }
            }

        }

        public override bool StartObject<T>(ref T obj)
        {
            if (obj == null)
            {
                SerializeNull();
                return false;
            }

            if (Session.GetRef(obj, out var id))
            {
                string refID = $"$ref|{id}";
                SerializeString(ref refID);
                return false;
            }

            var type = obj.GetType();

            ref byte addr = ref writer.GetMapHeader();

            propertyNum[depth] = 0;

            unsafe
            {
                lenAddr[depth] = (IntPtr)Unsafe.AsPointer(ref addr);
            }

            depth++;


            if (!type.IsValueType && !type.IsDefined(typeof(NoPolymorphicAttribute), false))
            {
                writer.Write("$type");
                writer.Write(type.FullName);
                PropertyNum++;

                id = Session.AddRefObject(obj);
                writer.Write("$id");
                writer.Write(id);
                PropertyNum++;
            }


            return true;
        }

        public override void EndObject()
        {
            depth--;

            unsafe
            {
                MessagePackWriter.WriteBigEndian(propertyNum[depth], (byte*)lenAddr[depth]);
            }
            
        }

        public override bool StartProperty(string name)
        {
            PropertyNum++;
            writer.Write(name);
            return true;
        }

        public override void EndProperty()
        {
        }

        public override bool StartArray<T>(ref T array, ref int len)
        {
            if (array == null)
            {
                writer.WriteNil();
                return false;
            }

            writer.WriteArrayHeader(len);
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
            writer.WriteNil();
        }

        public override void SerializePrimitive<T>(ref T val)
        {
            switch (val)
            {
                case bool v:
                    writer.Write(v);
                    break;
                case int v:
                    writer.Write(v);
                    break;
                case uint v:
                    writer.Write(v);
                    break;
                case float v:
                    writer.Write(v);
                    break;
                case double v:
                    writer.Write(v);
                    break;
                case long v:
                    writer.Write(v);
                    break;
                case ulong v:
                    writer.Write(v);
                    break;
                case short v:
                    writer.Write(v);
                    break;
                case ushort v:
                    writer.Write(v);
                    break;
                case char v:
                    writer.Write(v);
                    break;
                case sbyte v:
                    writer.Write(v);
                    break;
                case byte v:
                    writer.Write(v);
                    break;
                case decimal v:
                    //writer.Write(v);
                    break;
            }
            
        }

        public override void SerializeString(ref string val)
        {
            writer.Write(val);
        }

        public override void SerializeBytes(ref byte[] val)
        {
            writer.Write(val);
        }

        public override void SerializeGuid(ref Guid val)
        {
            unsafe
            {
                byte* pBytes = stackalloc byte[36];
                Span<byte> bytes = new Span<byte>(pBytes, 36);
                new GuidBits(ref val).Write(bytes);
                writer.WriteString(bytes);
            }
        }

        public override void SerializeUnmanaged<T>(ref T val, int count)
        {
            writer.WriteArrayHeader(count);

            for(int i = 0; i < count; i++)
            {
                SerializePrimitive(ref Unsafe.Add(ref val, i));
            }
        }

        public override void SerializeMemory<T>(ref IntPtr data, ref ulong length)
        {
            if (data == IntPtr.Zero)
            {
                writer.WriteNil();
            }
            else
            {
                unsafe
                {
                    writer.Write(new ReadOnlySpan<byte>((void*)data, (int)length));
                }
            }
        }
    }
}
