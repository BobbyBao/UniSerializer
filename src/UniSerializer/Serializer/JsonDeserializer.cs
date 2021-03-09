using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace UniSerializer
{
    public class JsonDeserializer : Deserializer
    {
        JsonDocument doc;
        JsonElement[] parentNodes = new JsonElement[32];
        int nodeCount = 0;
        JsonElement currentNode;
        public T Load<T>(string path) where T : new()
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                doc = JsonDocument.Parse(stream);
                parentNodes[nodeCount++] = doc.RootElement;
                currentNode = doc.RootElement;
                T obj = default;
                Serialize(ref obj);
                return obj;
            }
        }

        protected override object CreateObject()
        {
            if(currentNode.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if(!currentNode.TryGetProperty("$type" ,out var typeName))
            {
                return null;
            }

            var type = Type.GetType(typeName.GetString());

            return Activator.CreateInstance(type);
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
            parentNodes[nodeCount++] = currentNode;
            return true;
        }

        public override void SetElement(int index)
        {
            var parentNode = parentNodes[nodeCount - 1];
            currentNode = parentNode[index];
        }

        public override void EndArray()
        {
            currentNode = parentNodes[--nodeCount];
        }

        public override bool StartProperty(string name)
        {
            if (!currentNode.TryGetProperty(name, out var element))
            {
                return false;
            }

            parentNodes[nodeCount++] = currentNode;
            currentNode = element;
            return true;
        }

        public override void EndProperty()
        {
            currentNode = parentNodes[--nodeCount];            
        }

        public override void SerializeNull()
        {
        }

        public override void SerializePrimitive<T>(ref T val)
        {
            switch (val)
            {
                case bool v:
                    Unsafe.As<T, bool>(ref val)  = currentNode.GetBoolean();                    
                    break;
                case int v:
                    Unsafe.As<T, int>(ref val) = currentNode.GetInt32();
                    break;
                case uint v:
                    Unsafe.As<T, uint>(ref val) = currentNode.GetUInt32();
                    break;
                case float v:
                    Unsafe.As<T, float>(ref val) = currentNode.GetSingle();
                    break;
                case double v:
                    Unsafe.As<T, double>(ref val) = currentNode.GetDouble();
                    break;
                case long v:
                    Unsafe.As<T, long>(ref val) = currentNode.GetInt64();
                    break;
                case ulong v:
                    Unsafe.As<T, ulong>(ref val) = currentNode.GetUInt64();
                    break;
                case short v:
                    Unsafe.As<T, short>(ref val) = currentNode.GetInt16();
                    break;
                case ushort v:
                    Unsafe.As<T, ushort>(ref val) = currentNode.GetUInt16();
                    break;
                case char v:
                    Unsafe.As<T, char>(ref val) = (char)currentNode.GetUInt16();
                    break;
                case sbyte v:
                    Unsafe.As<T, sbyte>(ref val) = currentNode.GetSByte();
                    break;
                case byte v:
                    Unsafe.As<T, byte>(ref val) = currentNode.GetByte();
                    break;
                case decimal v:
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
    }
}
