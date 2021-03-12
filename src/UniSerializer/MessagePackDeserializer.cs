using MessagePack;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
            using var sequenceRental = SequencePool.Shared.Rent();
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
            }
            catch (Exception ex)
            {
                throw new MessagePackSerializationException("Error occurred while reading from the stream.", ex);
            }

            reader = new MessagePackReader(sequence);

            T obj = default;
            Serialize(ref obj);

            if (stream.CanSeek && !reader.End)
            {
                // Reverse the stream as many bytes as we left unread.
                int bytesNotRead = checked((int)reader.Sequence.Slice(reader.Position).Length);
                stream.Seek(-bytesNotRead, SeekOrigin.Current);
            }

            return obj;
        }


        protected override bool CreateObject(out object obj)
        {
            if(reader.TryReadStringSpan(out ReadOnlySpan<byte> span))
            {
                if (span.StartsWith(refCode))
                {
                    if(Utf8Parser.TryParse(span.Slice(5), out int id, out int bytesConsumed))
                    {
                        obj = Session.GetRefObject(id);
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

            if(!reader.TryReadMapHeader(out int count))
            {
                obj = null;
                return true;
            }

//             if(!reader.TryReadStringSpan(typeCode))
//             {
//                 obj = null;
//                 return true;
// 
//             }

            /*

            if (!currentNode.TryGetProperty("$type", out var typeName))
            {
                obj = null;
                return true;
            }

            var type = TypeUtilities.GetType(typeName.GetString());
            if (type == null)
            {
                obj = null;
                return true;
            }

            obj = Activator.CreateInstance(type);

            if (currentNode.TryGetProperty("$id", out var idNode))
            {
                var id = idNode.GetInt32();
                var id1 = Session.AddRefObject(obj);
                System.Diagnostics.Debug.Assert(id == id1);
            }
            else
            {
                //
            }*/

            obj = null;
            return true;
        }

        public override bool StartObject<T>(ref T obj)
        {
            if(reader.TryReadNil())
            {
                obj = default;
                return false;
            }

            return true;
        }

        public override void EndObject()
        {
        }

        public override bool StartProperty(string name)
        {
//             if (!currentNode.TryGetProperty(name, out var element))
//             {
//                 return false;
//             }
// 
//             parentNodes[nodeCount++] = currentNode;
//             currentNode = element;
            return true;
        }

        public override void EndProperty()
        {
            //currentNode = parentNodes[--nodeCount];
        }

        public override bool StartArray<T>(ref T array, ref int len)
        {
            if (reader.TryReadNil())
            {
                array = default;
                return false;
            }

            //             if (currentNode.ValueKind == JsonValueKind.Null)
            //             {
            //                 array = default;
            //                 return false;
            //             }
            // 
            //             if (currentNode.ValueKind != JsonValueKind.Array)
            //             {
            //                 System.Diagnostics.Debug.Assert(false);
            //                 return false;
            //             }
            // 
            //             len = currentNode.GetArrayLength();
            //             parentNodes[nodeCount++] = currentNode;
            return true;
        }

        public override void SetElement(int index)
        {
//             var parentNode = parentNodes[nodeCount - 1];
//             currentNode = parentNode[index];
        }

        public override void EndArray()
        {
            //currentNode = parentNodes[--nodeCount];
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
            val = reader.ReadString();
        }

        public override void SerializeBytes(ref byte[] val)
        {
            val = reader.ReadBytes()?.ToArray();
            //val = currentNode.GetBytesFromBase64();
        }
    }
}
