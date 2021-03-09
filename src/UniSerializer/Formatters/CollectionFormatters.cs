﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public sealed class ArrayFormatter<T> : Formatter<T[]>
    {
        public override void Serialize(ISerializer serialzer, ref T[] obj)
        {
            int len = obj?.Length ?? 0;
            if(!serialzer.StartArray(typeof(T[]), ref len))
            {
                return;
            }

            if (obj == null)
            {
                obj = new T[len];
            }

            for(int i = 0; i < len; i++)
            {
                serialzer.SetElement(i);
                serialzer.Serialize(ref obj[i]);
            }

            serialzer.EndArray();
        }
    }

    public class ListFormatter<T> : Formatter<List<T>>
    {
        public override void Serialize(ISerializer serialzer, ref List<T> obj)
        {
            int len = obj?.Count ?? 0;
            serialzer.StartArray(typeof(List<T>), ref len);

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
                    serialzer.Serialize(ref val);
                    obj.Add(val);
                }

            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    T val = obj[i];
                    serialzer.SetElement(i);
                    serialzer.Serialize(ref val);                    
                }

            }

            serialzer.EndArray();
        }
    }

    public class DictionaryFormatter<K, T> : Formatter<Dictionary<K, T>>
    {
        public override void Serialize(ISerializer serialzer, ref Dictionary<K, T> obj)
        {
            int len = obj?.Count ?? 0;
            serialzer.StartArray(typeof(Dictionary<K, T>), ref len);

            if (obj == null)
            {
                obj = new Dictionary<K, T>();
            }

            if (serialzer.IsReading)
            {
                obj.Clear();
                for (int i = 0; i < len/2; i++)
                {
                    K key = default;
                    T val = default;
                    serialzer.SetElement(2 * i);
                    serialzer.Serialize(ref key);
                    serialzer.SetElement(2 * i + 1);
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
                    //serialzer.SetElement(i);
                    serialzer.Serialize(ref k);
                    serialzer.Serialize(ref v);
                }

            }

            serialzer.EndArray();
        }
    }


}
