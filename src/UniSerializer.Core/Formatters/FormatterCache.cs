using System;
using System.Collections.Generic;

namespace UniSerializer
{
    public class FormatterCache
    {
        protected static Dictionary<Type, IFormatter> formatters = new Dictionary<Type, IFormatter>();

        public static IFormatter Get(Type type)
        {
            if (!formatters.TryGetValue(type, out var formatter))
            {
                formatter = CreateFormatter(type);
                formatters.Add(type, formatter);               
            }

            return formatter;
        }

        public static void Register(Type type, IFormatter formatter)
        {
            if (!formatters.TryGetValue(type, out var formatter1))
            {
                Log.Error("重复注册Formatter");
            }

            formatters.Add(type, formatter);
        }

        private static IFormatter CreateFormatter(Type type)
        {
            if (type.IsEnum)
            {
                Type instanceType = typeof(EnumFormatter<>).MakeGenericType(type);
                return Activator.CreateInstance(instanceType) as IFormatter;
            }
            else if (type.IsPrimitive)
            {
                Type instanceType = typeof(PrimitiveFormatter<>).MakeGenericType(type);
                return Activator.CreateInstance(instanceType) as IFormatter;
            }
            else if (type == typeof(string))
            {
                return new StringFormatter();
            }
            else if (type.IsValueType)
            {
                Type instanceType = typeof(ValueTypeFormatter<>).MakeGenericType(type);
                return Activator.CreateInstance(instanceType) as IFormatter;
            }

            if (type.IsArray)
            {
                Type instanceType = typeof(ArrayFormatter<>).MakeGenericType(type.GetElementType());
                return Activator.CreateInstance(instanceType) as IFormatter;
            }

            if (type.IsGenericType)
            {
                if (typeof(List<>) == type.GetGenericTypeDefinition())
                {
                    Type instanceType = typeof(ListFormatter<>).MakeGenericType(type.GetGenericArguments()[0]);
                    return Activator.CreateInstance(instanceType) as IFormatter;
                }
                else if (typeof(Dictionary<,>) == type.GetGenericTypeDefinition())
                {
                    var genericArgs = type.GetGenericArguments();
                    Type instanceType = typeof(DictionaryFormatter<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
                    return Activator.CreateInstance(instanceType) as IFormatter;

                }
            }
                            
            Type objectType = typeof(ObjectFormatter<>).MakeGenericType(type);
            return Activator.CreateInstance(objectType) as IFormatter;            

        }
    }

    public class FormatterCache<T> : FormatterCache
    {
        public static Formatter<T> Instance { get; }
        static FormatterCache()
        {
            Instance = (Formatter<T>)Get(typeof(T));
        }
    }

}
