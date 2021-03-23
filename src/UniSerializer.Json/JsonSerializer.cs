using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace UniSerializer
{
    public class JsonSerializer : Serializer
    {
        Utf8JsonWriter jsonWriter;

        public override void Save<T>(T obj, Stream stream)
        {
            JsonWriterOptions option = new JsonWriterOptions
            {
                Indented = true,
            };

            jsonWriter = new Utf8JsonWriter(stream, option);

            if (obj.GetType() != typeof(T))
            {
                object o = obj;
                Serialize(ref o);
            }
            else
                Serialize(ref obj);

            jsonWriter.Dispose();
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
            jsonWriter.WriteStartObject();
            if (!type.IsValueType)
            {
                jsonWriter.WriteString("$type", type.FullName);
                id = Session.AddRefObject(obj);
                jsonWriter.WriteNumber("$id", id);
            }

            return true;
        }

        public override void EndObject()
        {
            jsonWriter.WriteEndObject();
        }

        public override bool StartProperty(string name)
        {
            jsonWriter.WritePropertyName(name);
            return true;
        }

        public override void EndProperty()
        {
        }

        public override bool StartArray<T>(ref T array, ref int len)
        {
            if (array == null)
            {
                jsonWriter.WriteNullValue();
                return false;
            }

            jsonWriter.WriteStartArray();
            return true;
        }

        public override void SetElement(int index)
        {
        }

        public override void EndArray()
        {
            jsonWriter.WriteEndArray();
        }

        public override void SerializeNull()
        {
            jsonWriter.WriteNullValue();
        }

        public override void SerializePrimitive<T>(ref T val)
        {
            switch (val)
            {
                case bool v:
                    jsonWriter.WriteBooleanValue(v);
                    break;
                case int v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case uint v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case float v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case double v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case long v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case ulong v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case short v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case ushort v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case char v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case sbyte v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case byte v:
                    jsonWriter.WriteNumberValue(v);
                    break;
                case decimal v:
                    jsonWriter.WriteNumberValue(v);
                    break;
            }



        }

        public override void SerializeString(ref string val)
        {
            jsonWriter.WriteStringValue(val);
        }

        public override void SerializeBytes(ref byte[] val)
        {
            jsonWriter.WriteBase64StringValue(val);
        }

        public override void Serialize(ref Guid val)
        {
            jsonWriter.WriteStringValue(val);
        }

        public override void Serialize<T>(ref T val, int count)
        {
            using var sb = Cysharp.Text.ZString.CreateUtf8StringBuilder();
            for (int i = 0; i < count; i++)
            {
                if(i != 0)
                {
                    sb.Append(',');
                }

                sb.Append(val);
            }

            jsonWriter.WriteStringValue(sb.AsSpan());

        }
    }
}
