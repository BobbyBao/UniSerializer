using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public interface IFormatter
    {        
        void Serialize(Serializer visitor, ref object obj);
    }

    public class IFormatter<T> : IFormatter
    {
        public void Serialize(Serializer visitor, ref object obj)
        {
            Serialize(visitor, ref Unsafe.As<object, T>(ref obj));            
        }

        public virtual void Serialize(Serializer visitor, ref T obj)
        {

        }

    }

    public sealed class ArrayFormatter<T> : IFormatter<T[]>
    {
        public override void Serialize(Serializer serialzer, ref T[] obj)
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
        public override void Serialize(Serializer serialzer, ref List<T> obj)
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
        public override void Serialize(Serializer serialzer, ref Dictionary<K, T> obj)
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

    public class FormatterCache
    {
        protected static Dictionary<Type, IFormatter> formatters = new Dictionary<Type, IFormatter>();

        public static IFormatter Get(Type type)
        {
            if(formatters.TryGetValue(type, out var formatter))
            {
                return formatter;
            }

            if (type.IsArray)
            {
                Type instanceType = typeof(ArrayFormatter<>).MakeGenericType(type.GetElementType());
                formatter = Activator.CreateInstance(instanceType) as IFormatter;
            }

            if (type.IsGenericType)
            {
                if (typeof(List<>) == type.GetGenericTypeDefinition())
                {
                    Type instanceType = typeof(ListFormatter<>).MakeGenericType(type.GetGenericArguments()[0]);
                    formatter = Activator.CreateInstance(instanceType) as IFormatter;
                }
                else if (typeof(Dictionary<,>) == type.GetGenericTypeDefinition())
                {
                    var genericArgs = type.GetGenericArguments();
                    Type instanceType = typeof(Dictionary<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
                    formatter = Activator.CreateInstance(instanceType) as IFormatter;

                }
            }

            if (formatter == null)
            {
                Type instanceType = typeof(ObjectFormatter<>).MakeGenericType(type);
                formatter = Activator.CreateInstance(instanceType) as IFormatter;
            }

            formatters.Add(type, formatter);
            return formatter;
        }
    }

    internal class FormatterCache<T> : FormatterCache
    {
        public static IFormatter<T> instance;
        static FormatterCache()
        {
            Type type = typeof(T);
            instance = (IFormatter<T>)Get(type);

        }
    }

}
