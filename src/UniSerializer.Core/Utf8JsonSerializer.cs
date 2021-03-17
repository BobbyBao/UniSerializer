using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Utf8Json;

namespace UniSerializer
{
    public class Utf8JsonSerializer : Serializer
    {
        public const int MAX_DEPTH = 64;
        JsonWriter jsonWriter;
        int depth = 0;
        uint[] childrenCount = new uint[MAX_DEPTH];

        public bool Indented { get; set; } = true;
        private ref uint ChildrenCount => ref childrenCount[depth - 1];

        static readonly byte[][] indent = Enumerable.Range(0, 100).Select(x => Encoding.UTF8.GetBytes(new string(' ', x * 2))).ToArray();
        static readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        public override void Save<T>(T obj, Stream stream)
        {
            jsonWriter = new JsonWriter(MemoryPool.GetBuffer());

            if (obj.GetType() != typeof(T))
            {
                object o = obj;
                Serialize(ref o);
            }
            else
                Serialize(ref obj);

            stream.Write(jsonWriter.GetBuffer());
        }

        public override bool StartObject<T>(ref T obj)
        {
            if(obj == null)
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
            jsonWriter.WriteBeginObject();

            childrenCount[depth++] = 0;

            if (this.Indented)
            {
                jsonWriter.WriteRaw(newLine);
            }

            if (!type.IsValueType)
            {
                StartProperty("$type");
                jsonWriter.WriteString(type.FullName);
                EndProperty();

                id = Session.AddRefObject(obj);
                StartProperty("$id");
                jsonWriter.WriteInt32(id); 
                EndProperty();
            }

            return true;
        }

        public override void EndObject()
        {  
            depth--;

            if (this.Indented)
            {
                jsonWriter.WriteRaw(newLine);
                jsonWriter.WriteRaw(indent[depth]);
            }
    
            jsonWriter.WriteEndObject();
        }

        public override bool StartProperty(string name)
        {
            if (ChildrenCount > 0)
            {
                jsonWriter.WriteValueSeparator();

                if (this.Indented)
                {
                    jsonWriter.WriteRaw(newLine);
                }
            }

            if (this.Indented)
            {
                jsonWriter.WriteRaw(indent[depth]);
            }

            jsonWriter.WritePropertyName(name);

            if (this.Indented)
            {
                jsonWriter.WriteRaw((byte)' ');
            }

            return true;
        }

        public override void EndProperty()
        {
            ChildrenCount++;
        }

        public override bool StartArray<T>(ref T array, ref int len)
        {
            if(array == null)
            {
                jsonWriter.WriteNull();
                return false;
            }

            jsonWriter.WriteBeginArray();

            if (this.Indented)
            {
                jsonWriter.WriteRaw(newLine);
            }

            childrenCount[depth++] = 0;
            return true;
        }

        public override void SetElement(int index)
        {
            if (index > 0)
            {
                jsonWriter.WriteRaw((byte)',');

                if (this.Indented)
                {
                    jsonWriter.WriteRaw(newLine);
                }
            }

            if (this.Indented)
            {
                jsonWriter.WriteRaw(indent[depth]);
            }
        }

        public override void EndArray()
        {   
            depth--;

            if (this.Indented)
            {
                jsonWriter.WriteRaw(newLine);
                jsonWriter.WriteRaw(indent[depth]);
            }
    
            jsonWriter.WriteEndArray();
        }

        public override void SerializeNull()
        {
            jsonWriter.WriteNull();
        }

        public override void SerializePrimitive<T>(ref T val)
        {
            switch (val)
            {
                case bool v:
                    jsonWriter.WriteBoolean(v);
                    break;
                case int v:
                    jsonWriter.WriteInt32(v);
                    break;
                case uint v:
                    jsonWriter.WriteUInt32(v);
                    break;
                case float v:
                    jsonWriter.WriteSingle(v);
                    break;
                case double v:
                    jsonWriter.WriteDouble(v);
                    break;
                case long v:
                    jsonWriter.WriteInt64(v);
                    break;
                case ulong v:
                    jsonWriter.WriteUInt64(v);
                    break;
                case short v:
                    jsonWriter.WriteInt16(v);
                    break;
                case ushort v:
                    jsonWriter.WriteUInt16(v);
                    break;
                case char v:
                    jsonWriter.WriteUInt16(v);
                    break;
                case sbyte v:
                    jsonWriter.WriteSByte(v);
                    break;
                case byte v:
                    jsonWriter.WriteByte(v);
                    break;
                case decimal v:
                    //jsonWriter.WriteNumberValue(v);
                    break;
            }
                


        }

        public override void SerializeString(ref string val)
        {
            jsonWriter.WriteString(val);
        }

        public override void SerializeBytes(ref byte[] val)
        {
            //jsonWriter.WriteBase64StringValue(val);
        }

    }

    static class MemoryPool
    {
        [ThreadStatic]
        static byte[] buffer = null;

        public static byte[] GetBuffer()
        {
            if (buffer == null)
            {
                buffer = new byte[65536];
            }
            return buffer;
        }
    }
}
