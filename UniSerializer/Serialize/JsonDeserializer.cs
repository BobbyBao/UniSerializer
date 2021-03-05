using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace UniSerializer
{
    public class JsonDeserializer : Serializer
    {
        JsonDocument doc;
        JsonElement[] nodes = new JsonElement[32];
        int nodeCount = 0;
        public T Load<T>(string path) where T : new()
        {
            using var stream = new FileStream(path, FileMode.Open);

            doc = JsonDocument.Parse(stream);
            nodes[nodeCount++] = doc.RootElement;
            T obj = default;
            Serialize(ref obj);
            return obj;
        }

    }
}
