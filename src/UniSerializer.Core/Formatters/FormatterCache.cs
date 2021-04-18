using System;
using System.Collections.Generic;

namespace UniSerializer
{
    public interface IFormatterFactory
    {
        IFormatter CreateFormatter(Type type);
    }

    public class FormatterCache
    {
        protected static Dictionary<Type, IFormatter> formatters = new Dictionary<Type, IFormatter>();
        protected static List<IFormatterFactory> formatterFactories = new List<IFormatterFactory>();
        static FormatterCache()
        {
            Register(typeof(Guid), new GuidFormatter());
            Register(typeof(byte[]), new BytesFormatter());


            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var attrUncast in ass.GetCustomAttributes(typeof(RegisterFormatterAttribute), true))
                {
                    var attr = (RegisterFormatterAttribute)attrUncast;

                    if (!attr.FormatterType.IsClass
                        || attr.FormatterType.IsAbstract
                        || attr.FormatterType.GetConstructor(Type.EmptyTypes) == null
                        || !typeof(Formatter<>).IsAssignableFrom(attr.FormatterType))
                    {
                        continue;
                    }

                    var TargetType = attr.FormatterType.GetArgumentsOfInheritedOpenGenericInterface(typeof(Formatter<>))[0];
                    try
                    {
                        Register(TargetType, (IFormatter)Activator.CreateInstance(attr.FormatterType));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Exception was thrown while instantiating Formatter of type ", attr.FormatterType.FullName, ".", ex.Message);
                    }

                }

                foreach (var attrUncast in ass.GetCustomAttributes(typeof(RegisterFormatterFactoryAttribute), true))
                {
                    var attr = (RegisterFormatterFactoryAttribute)attrUncast;

                    if (!attr.FormatterFactoryType.IsClass
                        || attr.FormatterFactoryType.IsAbstract
                        || attr.FormatterFactoryType.GetConstructor(Type.EmptyTypes) == null
                        || !typeof(IFormatterFactory).IsAssignableFrom(attr.FormatterFactoryType))
                    {
                        continue;
                    }
                    try
                    {
                        RegisterFactory((IFormatterFactory)Activator.CreateInstance(attr.FormatterFactoryType));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Exception was thrown while instantiating FormatterLocator of type ", attr.FormatterFactoryType.FullName, ".", ex.Message);
                    }

                }
            }

            Register(typeof(Guid), new GuidFormatter());
        }
            
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
            if (formatters.ContainsKey(type))
            {
                Log.Error("重复注册Formatter");
                return;
            }

            formatters.Add(type, formatter);
        }

        public static void RegisterFactory(IFormatterFactory factory)
        {
            formatterFactories.Add(factory);
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

            foreach(var factory in formatterFactories)
            { 
                var formatter = factory.CreateFormatter(type);
                if(formatter != null)
                {
                    return formatter;
                }
            }

            if (type.IsValueType)
            {
                Type instanceType = typeof(ValueTypeFormatter<>).MakeGenericType(type);
                return Activator.CreateInstance(instanceType) as IFormatter;
            }
            else
            {
                if (type.IsAbstract)
                {
                    Type objectType = typeof(AbstractObjectFormatter<>).MakeGenericType(type);
                    return Activator.CreateInstance(objectType) as IFormatter;
                }
                else
                {

                    Type objectType = typeof(ObjectFormatter<>).MakeGenericType(type);
                    return Activator.CreateInstance(objectType) as IFormatter;
                }

            }           

        }
    }

    public class FormatterCache<T> : FormatterCache
    {
        static FormatterCache()
        {
            Formatter<T>.Instance = (Formatter<T>)Get(typeof(T));
        }

        public static Formatter<T> Instance => Formatter<T>.Instance;

        public static void Register(Formatter<T> formatter)
        {
            Register(typeof(T), formatter);
            Formatter<T>.Instance = formatter;
        }
    }

}
