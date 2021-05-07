using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Utf8Json;

namespace UniSerializer
{
    public class Utf8JsonDeserializer : Deserializer
    {
        public const int MAX_DEPTH = 64;

        SerializeValue doc;
        SerializeValue[] parentNodes = new SerializeValue[MAX_DEPTH];
        int depth = 0;
        SerializeValue currentNode;        
        public override T Load<T>(Stream stream)
        {
            var buf = MemoryPool.GetBuffer();
            var len = FillFromStream(stream, ref buf);

            // when token is number, can not use from pool(can not find end line).
            var token = new JsonReader(buf).GetCurrentJsonToken();
            if (token == JsonToken.Number)
            {
                buf = Utf8Json.Internal.BinaryUtil.FastCloneWithResize(buf, len);
            }

            var reader = new JsonReader(buf);
            doc = Parse(ref reader, 0);
            parentNodes[depth++] = doc;
            currentNode = doc;
            T obj = default;
            Serialize(ref obj, 1);
            return obj;
        }

        static int FillFromStream(Stream input, ref byte[] buffer)
        {
            int length = 0;
            int read;
            while ((read = input.Read(buffer, length, buffer.Length - length)) > 0)
            {
                length += read;
                if (length == buffer.Length)
                {
                    Utf8Json.Internal.BinaryUtil.FastResize(ref buffer, length * 2);
                }
            }

            return length;
        }

        static SerializeValue Parse(ref JsonReader reader,  int depth)
        {
            SerializeValue value = default;
            var token = reader.GetCurrentJsonToken();
            switch (token)
            {
                case JsonToken.BeginObject:
                    {
                        value = new SerializeValue(new Dictionary<string, SerializeValue>());                        
                        var c = 0;
                        while (reader.ReadIsInObject(ref c))
                        {
                            var propName = reader.ReadPropertyName();
                            var element = Parse(ref reader, depth + 1);
                            value.Add(propName, element);
                        }

                    }
                    break;
                case JsonToken.BeginArray:
                    {
                        value = new SerializeValue(ValueType.Array);
                        value.array = new List<SerializeValue>();

                        var c = 0;
                        while (reader.ReadIsInArray(ref c))
                        {
                            var element = Parse(ref reader, depth + 1);
                            value.Add(element);
                        }

                    }
                    break;
                case JsonToken.Number:
                    {
                        value = reader.ReadDouble();
                    }
                    break;
                case JsonToken.String:
                    {
                        value = reader.ReadString();
                    }
                    break;
                case JsonToken.True:
                case JsonToken.False:
                    {
                        value = reader.ReadBoolean();
                    }
                    break;
                case JsonToken.Null:
                    {
                        reader.ReadIsNull();
                        value = SerializeValue.Null;
                    }
                    break;
                default:
                    break;
            }

            return value;
        }

        protected override bool CreateObject(out object obj)
        {
            if(currentNode.type == ValueType.String)
            {
                var str = (string)currentNode;
                if(str.StartsWith("$ref|"))
                {
#if NETSTANDARD
                    var id = int.Parse(str.Substring(5));
#else
                    var id = int.Parse(str.AsSpan(5));
#endif
                    obj = Session.GetRefObject(id);
                    return false;
                }
                else
                {
                    obj = null;
                    return true;
                }

            }

            if(currentNode.type != ValueType.Map)
            {
                obj = null;
                return true;
            }

            if(!currentNode.TryGet("$type", out var typeName))
            {
                obj = null;
                return true;
            }

            var type = TypeUtilities.GetType((string)typeName);
            if(type == null)
            {
                obj = null;
                return true;
            }

            obj = Activator.CreateInstance(type);

            if (currentNode.TryGet("$id", out var idNode))
            {
                var id = (int)idNode;
                var id1 = Session.AddRefObject(obj);
                System.Diagnostics.Debug.Assert(id == id1);
            }
            else
            {
                Log.Error("no $id property");
                //
            }

            return true;
        }

        public override bool StartObject<T>(ref T obj)
        {
            if(currentNode.type == ValueType.Null)
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
            if (!currentNode.TryGet(name, out var element))
            {
                return false;
            }

            parentNodes[depth++] = currentNode;
            currentNode = element;
            return true;
        }

        public override void EndProperty()
        {
            currentNode = parentNodes[--depth];
        }

        public override bool StartArray<T>(ref T array, ref int len)
        {
            if (currentNode.type == ValueType.Null)
            {
                array = default;
                return false;
            }

            if (currentNode.type != ValueType.Array)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            len = currentNode.ElementCount;
            parentNodes[depth++] = currentNode;
            return true;
        }

        public override void SetElement(int index)
        {
            var parentNode = parentNodes[depth - 1];
            currentNode = parentNode[index];
        }

        public override void EndArray()
        {
            currentNode = parentNodes[--depth];
        }

        public override void SerializeNull()
        {
        }

        public override void SerializePrimitive<T>(ref T val)
        {
            switch (val)
            {
                case bool _:
                    Unsafe.As<T, bool>(ref val)  = (bool)currentNode;                    
                    break;
                case int _:
                    Unsafe.As<T, int>(ref val) = (int)currentNode;
                    break;
                case uint _:
                    Unsafe.As<T, uint>(ref val) = (uint)currentNode;
                    break;
                case float _:
                    Unsafe.As<T, float>(ref val) = (float)currentNode;
                    break;
                case double _:
                    Unsafe.As<T, double>(ref val) = (float)currentNode;
                    break;
                case long _:
                    Unsafe.As<T, long>(ref val) = (long)currentNode;
                    break;
                case ulong _:
                    Unsafe.As<T, ulong>(ref val) = (ulong)currentNode;
                    break;
                case short _:
                    Unsafe.As<T, short>(ref val) = (short)currentNode;
                    break;
                case ushort _:
                    Unsafe.As<T, ushort>(ref val) = (ushort)currentNode;
                    break;
                case char _:
                    Unsafe.As<T, char>(ref val) = (char)currentNode;
                    break;
                case sbyte _:
                    Unsafe.As<T, sbyte>(ref val) = (sbyte)currentNode;
                    break;
                case byte _:
                    Unsafe.As<T, byte>(ref val) = (byte)currentNode;
                    break;
                case decimal _:
                    //Unsafe.As<T, decimal>(ref val) = currentNode.GetDecimal();
                    break;
            }

        }

        public override void SerializeString(ref string val)
        {
            val = (string)currentNode;
        }

        public override void SerializeBytes(ref byte[] val)
        {
            //val = currentNode.GetBytesFromBase64();
        }

        public override void SerializeGuid(ref Guid val)
        {
            throw new NotImplementedException();
        }

        public override void SerializeUnmanaged<T>(ref T val, int count)
        {
            throw new NotImplementedException();
        }

        public override void SerializeMemory(ref IntPtr data, ref ulong length)
        {
            throw new NotImplementedException();
        }
    }
}
