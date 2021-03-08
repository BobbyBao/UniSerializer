using System;
using System.Collections.Generic;
using System.IO;
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
            using var stream = new FileStream(path, FileMode.Open);

            doc = JsonDocument.Parse(stream);
            parentNodes[nodeCount++] = doc.RootElement;
            currentNode = doc.RootElement;
            T obj = default;
            Serialize(ref obj);
            return obj;
        }

        protected override object CreateObject()
        {
            if(!currentNode.TryGetProperty("$type" ,out var typeName))
            {
                return null;
            }

            var type = Type.GetType(typeName.GetString());

            return Activator.CreateInstance(type);
        }

        public override bool StartObject(System.Type type)
        {
            return true;
        }

        public override void EndObject()
        {
        }

        public override bool StartArray(System.Type type, ref int len)
        {
            if (currentNode.ValueKind != JsonValueKind.Array)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            len = currentNode.GetArrayLength();
            parentNodes[nodeCount++] = currentNode;
            return false;
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

        protected override void SerializePrimitive<T>(ref T val)
        {
        }
    }
}
