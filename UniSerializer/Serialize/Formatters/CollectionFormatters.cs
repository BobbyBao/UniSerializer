using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public sealed class ArrayFormatter<T> : IFormatter<T[]>
    {
        public override void Serialize(ISerializer serialzer, ref T[] obj)
        {
            int len = obj?.Length ?? 0;
            serialzer.StartArray(ref len);

            if (obj == null)
            {
                obj = new T[len];
            }

            for(int i = 0; i < len; i++)
            {
                serialzer.Serialize(ref obj[i]);
            }

            serialzer.EndArray();
        }
    }

    public class ListFormatter<T> : IFormatter<List<T>>
    {
        public override void Serialize(ISerializer serialzer, ref List<T> obj)
        {
            int len = obj?.Count ?? 0;
            serialzer.StartArray(ref len);

            if(serialzer.IsReading)
            {
                obj.Clear();
                for (int i = 0; i < len; i++)
                {
                    T val = default;
                    serialzer.Serialize(ref val);
                    obj.Add(val);
                }

            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    T val = obj[i];
                    serialzer.Serialize(ref val);                    
                }

            }

            serialzer.EndArray();
        }
    }

    public class DictionaryFormatter<K, T> : IFormatter<Dictionary<K, T>>
    {
        public override void Serialize(ISerializer serialzer, ref Dictionary<K, T> obj)
        {
            int len = obj?.Count ?? 0;
            serialzer.StartArray(ref len);

            if (serialzer.IsReading)
            {
                obj.Clear();
                for (int i = 0; i < len; i++)
                {
                    K key = default;
                    T val = default;
                    serialzer.Serialize(ref key);
                    serialzer.Serialize(ref val);
                    obj[key] = val;
                }

            }
            else
            {
                foreach (var kvp in obj)
                {
                    K k = kvp.Key;
                    T v = kvp.Value; 
                    serialzer.Serialize(ref k);
                    serialzer.Serialize(ref v);
                }

            }

            serialzer.EndArray();
        }
    }


}
