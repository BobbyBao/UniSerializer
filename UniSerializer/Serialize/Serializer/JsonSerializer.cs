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
        System.Text.Json.Utf8JsonWriter jsonWriter;
        public void Save<T>(T obj, string path)
        {
            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                JsonWriterOptions option = new JsonWriterOptions
                {
                    Indented = true,
                };

                jsonWriter = new System.Text.Json.Utf8JsonWriter(stream, option);
               
                Serialize(ref obj);
                jsonWriter.Dispose();
            }
        }

        public override void StartObject()
        {               
            jsonWriter.WriteStartObject();
        }

        public override void EndObject()
        {
            jsonWriter.WriteEndObject();
        }

        public override void StartArray(ref int len)
        {               
            jsonWriter.WriteStartArray();
        }

        public override void EndArray()
        {
            jsonWriter.WriteEndArray();
        }

        public override void StartProperty(string name)
        {
            jsonWriter.WritePropertyName(name);
        }

        public override void EndProperty()
        {
        }

        protected override void SerializePrimitive<T>(ref T val)
        {
            switch (val)
            {
                case int intVal:
                    jsonWriter.WriteNumberValue(intVal);
                    break;
                case uint uintVal:
                    jsonWriter.WriteNumberValue(uintVal);
                    break;
                case float floatVal:
                    jsonWriter.WriteNumberValue(floatVal);
                    break;
                case double doubleVal:
                    jsonWriter.WriteNumberValue(doubleVal);
                    break;
            }




        }

    }
}
