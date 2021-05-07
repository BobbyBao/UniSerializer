using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace UniSerializer
{
    public class JsonDeserializer : Deserializer
    {
        public const int MAX_DEPTH = 64;

        JsonDocument doc;
        JsonElement[] parentNodes = new JsonElement[MAX_DEPTH];
        int depth = 0;
        JsonElement currentNode;        
        public override T Load<T>(Stream stream)
        {
            doc = JsonDocument.Parse(stream);
            parentNodes[depth++] = doc.RootElement;
            currentNode = doc.RootElement;
            T obj = default;
            Serialize(ref obj, 1);
            return obj;
        }

        protected override bool CreateObject(out object obj)
        {
            if(currentNode.ValueKind == JsonValueKind.String)
            {
                var str = currentNode.GetString();
                if(str.StartsWith("$ref|"))
                {
                    var id = int.Parse(str.AsSpan(5));
                    obj = Session.GetRefObject(id);
                    return false;
                }
                else
                {
                    obj = null;
                    return true;
                }

            }

            if(currentNode.ValueKind != JsonValueKind.Object)
            {
                obj = null;
                return true;
            }

            if(!currentNode.TryGetProperty("$type" ,out var typeName))
            {
                obj = null;
                return true;
            }

            var type = TypeUtilities.GetType(typeName.GetString());
            if(type == null)
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
            }

            return true;
        }

        public override bool StartObject<T>(ref T obj)
        {
            if(currentNode.ValueKind == JsonValueKind.Null)
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
            if (!currentNode.TryGetProperty(name, out var element))
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
            if (currentNode.ValueKind == JsonValueKind.Null)
            {
                array = default;
                return false;
            }

            if (currentNode.ValueKind != JsonValueKind.Array)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            len = currentNode.GetArrayLength();
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
                case bool:
                    Unsafe.As<T, bool>(ref val)  = currentNode.GetBoolean();                    
                    break;
                case int:
                    Unsafe.As<T, int>(ref val) = currentNode.GetInt32();
                    break;
                case uint:
                    Unsafe.As<T, uint>(ref val) = currentNode.GetUInt32();
                    break;
                case float:
                    Unsafe.As<T, float>(ref val) = currentNode.GetSingle();
                    break;
                case double:
                    Unsafe.As<T, double>(ref val) = currentNode.GetDouble();
                    break;
                case long:
                    Unsafe.As<T, long>(ref val) = currentNode.GetInt64();
                    break;
                case ulong:
                    Unsafe.As<T, ulong>(ref val) = currentNode.GetUInt64();
                    break;
                case short:
                    Unsafe.As<T, short>(ref val) = currentNode.GetInt16();
                    break;
                case ushort:
                    Unsafe.As<T, ushort>(ref val) = currentNode.GetUInt16();
                    break;
                case char:
                    Unsafe.As<T, char>(ref val) = (char)currentNode.GetUInt16();
                    break;
                case sbyte:
                    Unsafe.As<T, sbyte>(ref val) = currentNode.GetSByte();
                    break;
                case byte:
                    Unsafe.As<T, byte>(ref val) = currentNode.GetByte();
                    break;
                case decimal:
                    Unsafe.As<T, decimal>(ref val) = currentNode.GetDecimal();
                    break;
            }

        }

        public override void SerializeString(ref string val)
        {
            val = currentNode.GetString();
        }

        public override void SerializeBytes(ref byte[] val)
        {
            val = currentNode.GetBytesFromBase64();
        }

        public override void SerializeGuid(ref Guid val)
        {
            val = currentNode.GetGuid();
        }

        public override void SerializeUnmanaged<T>(ref T val, int count)
        {
            var str = currentNode.GetString().AsSpan();

            int start = 0;
            int elementCount = 0;

            switch (val)
            {
                case int:
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (str[i] == ',')
                        {
                            var v = int.Parse(str.Slice(start, i - start));
                            Unsafe.As<T, int>(ref Unsafe.Add(ref val, elementCount)) = v;
                            elementCount++;
                            start = i + 1;
                        }
                        else if(i == str.Length - 1)
                        {
                            var v = int.Parse(str.Slice(start, i - start + 1));
                            Unsafe.As<T, int>(ref Unsafe.Add(ref val, elementCount)) = v;
                            elementCount++;
                        }
                        
                    }

                    break;
                case uint:
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (str[i] == ',')
                        {
                            var v = uint.Parse(str.Slice(start, i - start));
                            Unsafe.As<T, uint>(ref Unsafe.Add(ref val, elementCount)) = v;
                            elementCount++;
                            start = i + 1;
                        }
                        else if (i == str.Length - 1)
                        {
                            var v = uint.Parse(str.Slice(start, i - start + 1));
                            Unsafe.As<T, uint>(ref Unsafe.Add(ref val, elementCount)) = v;
                            elementCount++;
                        }
                    }

                    break;
                case float:
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (str[i] == ',')
                        {
                            var v = float.Parse(str.Slice(start, i - start));
                            Unsafe.As<T, float>(ref Unsafe.Add(ref val, elementCount)) = v;
                            elementCount++;
                            start = i + 1;
                        }
                        else if (i == str.Length - 1)
                        {
                            var v = float.Parse(str.Slice(start, i - start + 1));
                            Unsafe.As<T, float>(ref Unsafe.Add(ref val, elementCount)) = v;
                            elementCount++;
                        }
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }

            System.Diagnostics.Debug.Assert(count == elementCount);

        }

        public override void SerializeMemory(ref IntPtr data, ref ulong length)
        {
            if(currentNode.ValueKind == JsonValueKind.Null)
            {
                data = IntPtr.Zero;
                length = 0;
                return;
            }

            if(currentNode.ValueKind == JsonValueKind.String)
            {
                if(currentNode.TryGetBytesFromBase64(out var bytes))
                {
                    data = Marshal.AllocHGlobal(bytes.Length);
                    unsafe
                    {
                        Unsafe.CopyBlockUnaligned((void*)data, Unsafe.AsPointer(ref bytes[0]), (uint)bytes.Length);
                    }
                }

            }

        }
    }
}
