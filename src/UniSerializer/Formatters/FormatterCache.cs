using System;
using System.Collections.Generic;

namespace UniSerializer
{
    public class FormatterCache
    {
        protected static Dictionary<Type, IFormatter> formatters = new Dictionary<Type, IFormatter>();

        public static IFormatter Get(Type type)
        {
            if(formatters.TryGetValue(type, out var formatter))
            {
                return formatter;
            }

            if(type.IsEnum)
            {
                Type instanceType = typeof(EnumFormatter<>).MakeGenericType(type);
                formatter = Activator.CreateInstance(instanceType) as IFormatter;
            }
            else if(type.IsPrimitive)
            {
                Type instanceType = typeof(PrimitiveFormatter<>).MakeGenericType(type);
                formatter = Activator.CreateInstance(instanceType) as IFormatter;
            }
            else if(type == typeof(string))
            {
                formatter = new StringFormatter();
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
                    Type instanceType = typeof(DictionaryFormatter<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
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

    public class FormatterCache<T> : FormatterCache
    {
        public static Formatter<T> Instance { get; }
        static FormatterCache()
        {
            Type type = typeof(T);
            Instance = (Formatter<T>)Get(type);

        }
    }

}
