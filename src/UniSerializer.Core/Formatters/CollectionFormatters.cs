using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public sealed class ArrayFormatter<T> : Formatter<T[]>
    {
        public override void Serialize(ISerializer serialzer, ref T[] obj, uint flags)
        {
            int len = obj?.Length ?? 0;
            if(!serialzer.StartArray(ref obj, ref len))
            {
                return;
            }

            if (obj == null)
            {
                obj = new T[len];
            }

            if(obj.Length < len)
            {
                Array.Resize(ref obj, len);
            }

            for (int i = 0; i < len; i++)
            {
                serialzer.SetElement(i);
                serialzer.Serialize(ref obj[i], 0);
            }           

            serialzer.EndArray();
        }
    }

    public class ListFormatter<T> : Formatter<List<T>>
    {
        public override void Serialize(ISerializer serialzer, ref List<T> obj, uint flags)
        {
            int len = obj?.Count ?? 0;
            if(!serialzer.StartArray(ref obj, ref len))
            {
                return;
            }

            if (obj == null)
            {
                obj = new List<T>();
            }

            if (serialzer.IsReading)
            {
                obj.Clear();
                for (int i = 0; i < len; i++)
                {
                    T val = default;
                    serialzer.SetElement(i);
                    serialzer.Serialize(ref val, 0);
                    obj.Add(val);
                }

            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    T val = obj[i];
                    serialzer.SetElement(i);
                    serialzer.Serialize(ref val, 0);                    
                }

            }

            serialzer.EndArray();
        }
    }

    public class DictionaryFormatter<K, T> : Formatter<Dictionary<K, T>>
    {
        public override void Serialize(ISerializer serialzer, ref Dictionary<K, T> obj, uint flags)
        {
            int len = 0;
            if(obj != null)
            {
                len = obj.Count * 2;
            }

            if(!serialzer.StartArray(ref obj, ref len))
            {
                return;
            }

            if (obj == null)
            {
                obj = new Dictionary<K, T>();
            }

            if (serialzer.IsReading)
            {
                obj.Clear();
                for (int i = 0; i < len;)
                {
                    K key = default;
                    T val = default;
                    serialzer.SetElement(i++);
                    serialzer.Serialize(ref key, 0);
                    serialzer.SetElement(i++);
                    serialzer.Serialize(ref val, 0);
                    obj[key] = val;
                }

            }
            else
            {
                int i = 0;
                foreach (var kvp in obj)
                {
                    K k = kvp.Key;
                    T v = kvp.Value;
                    serialzer.SetElement(i++);
                    serialzer.Serialize(ref k, 0);
                    serialzer.SetElement(i++);
                    serialzer.Serialize(ref v, 0);
                }

            }

            serialzer.EndArray();
        }
    }


}
