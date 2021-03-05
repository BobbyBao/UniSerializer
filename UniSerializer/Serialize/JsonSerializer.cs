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

        public override void Serialize<T>(ref T val)
        {
            Type type = typeof(T);
            if (type.IsPrimitive)
            {
                SerializePrimitive(ref val);
            }
            else
            {
                SerializeObject(ref val);
            }
        }

        public override void Serialize(ref object val)
        {
            Type type = val.GetType();

            if (type.IsPrimitive)
            {
                SerializePrimitive(ref val);
            }
            else
            {
                SerializeObject(ref val);
            }
        }

        public override void SerializeObject<T>(ref T obj)
        {
            
            if (obj is ISerializable ser)
            {
                StartObject();
                ser.Serialize(this);
                EndObject();
            }
            else
            {
                Type type = obj.GetType();
                if(type != typeof(T))
                {                    
                    FormatterCache.Get(type).Serialize(this, ref Unsafe.As<T, Object>(ref obj));
                }
                else
                    FormatterCache<T>.instance.Serialize(this, ref obj);
            }

        }

        public override void SerializeProperty<T>(string name, ref T val)
        {
            StartAttribute(name);

            Serialize(ref val);

            EndAttribute();
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

        public override void StartAttribute(string name)
        {
            jsonWriter.WritePropertyName(name);
        }

        public override void EndAttribute()
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
