using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public class MessagePackSerializer : Serializer
    {
        MessagePackWriter writer;

        int depth = 0;
        uint[] lens = new uint[64];
        IntPtr[] lenAddr = new IntPtr[64];

        private ref uint PropertyCount => ref lens[depth - 1];

        public override void Save<T>(T obj, Stream stream)
        {
            using SequencePool.Rental sequenceRental = SequencePool.Shared.Rent();
            writer = new MessagePackWriter(sequenceRental.Value);

            if (obj.GetType() != typeof(T))
            {
                object o = obj;
                Serialize(ref o);
            }
            else
                Serialize(ref obj);

            try
            {
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

            lens[depth] = 0;

            unsafe
            {
                lenAddr[depth] = (IntPtr)Unsafe.AsPointer(ref addr);
            }

            depth++;


            if (!type.IsValueType)
            {
                writer.Write("$type");
                writer.Write(type.FullName);
                PropertyCount++;

                id = Session.AddRefObject(obj);
                writer.Write("$id");
                writer.Write(id);
                PropertyCount++;
            }


            return true;
        }

        public override void EndObject()
        {
            depth--;

            unsafe
            {
                MessagePackWriter.WriteBigEndian(lens[depth], (byte*)lenAddr[depth]);
            }
            
        }

        public override bool StartProperty(string name)
        {
            PropertyCount++;
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

    }
}
