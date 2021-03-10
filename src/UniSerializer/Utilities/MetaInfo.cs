using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace UniSerializer
{
    public class MetaInfo : Dictionary<string, MemberAccessor>
    {
        static ConcurrentDictionary<Type, MetaInfo> metaInfoDB = new ConcurrentDictionary<Type, MetaInfo>();
        private readonly Type type;
        public MetaInfo(Type type)
        {
            this.type = type;
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var hash = new HashCode();
            foreach (var p in properties)
            {
                if (p.GetMethod == null || p.SetMethod == null)
                {
                    continue;
                }

                if (p.IsDefined(typeof(NonSerializedAttribute)))
                {
                    continue;
                }

                if (p.IsDefined(typeof(IgnoreDataMemberAttribute)))
                {
                    continue;
                }

                Add(p.Name, CreatePropertyAccessor(type.IsValueType, p));
                hash.Add(p.Name);
            }

            HashCode = hash.ToHashCode();
        }

        public Type Type => type;
        public int HashCode { get; }

        public static MemberAccessor CreatePropertyAccessor(bool valueType, PropertyInfo propertyInfo)
        {
            Type instanceType = valueType ? typeof(ValueMemberAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType)
                 : typeof(ObjectMemberAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (MemberAccessor)Activator.CreateInstance(instanceType, propertyInfo);
        }

        public static MetaInfo Get<T>() => Get(typeof(T));

        public static MetaInfo Get(Type type)
        {
            if(!metaInfoDB.TryGetValue(type, out var metaInfo))
            {
                metaInfo = new MetaInfo(type);
                metaInfoDB.TryAdd(type, metaInfo);
            }

            return metaInfo;
        }

    }

}
