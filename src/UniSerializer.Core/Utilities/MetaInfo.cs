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

            if((SerializationConfig.SerializationMode & SerializationMode.Properties) != 0)
            {
                AddProperties();
            }
            
            if((SerializationConfig.SerializationMode & SerializationMode.Fields) != 0 && EmitUtilities.CanEmit)
            {
                AddFields();
            }
        }

        public Type Type => type;
        public string TypeName => type.Name;

        private void AddProperties()
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetMethod == null || propertyInfo.SetMethod == null)
                {
                    continue;
                }

                if (propertyInfo.IsDefined(typeof(NonSerializedAttribute)))
                {
                    continue;
                }

                if (propertyInfo.IsDefined(typeof(IgnoreDataMemberAttribute)))
                {
                    continue;
                }

                Add(propertyInfo.Name, CreatePropertyAccessor(type.IsValueType, propertyInfo));

            }

        }

        private void AddFields()
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.IsDefined(typeof(NonSerializedAttribute)))
                {
                    continue;
                }

                if (fieldInfo.IsDefined(typeof(IgnoreDataMemberAttribute)))
                {
                    continue;
                }
                
                if (!fieldInfo.IsPublic && !fieldInfo.IsDefined(typeof(DataMemberAttribute)) && !fieldInfo.IsDefined(typeof(SerializedFieldAttribute)))
                {
                    continue;
                }

                Add(fieldInfo.Name, CreateFieldAccessor(type.IsValueType, fieldInfo));

            }

        }

        private static MemberAccessor CreatePropertyAccessor(bool valueType, PropertyInfo propertyInfo)
        {
            Type instanceType = valueType ? typeof(ValueMemberAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType)
                 : typeof(ObjectMemberAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (MemberAccessor)Activator.CreateInstance(instanceType, propertyInfo);
        }

        private static MemberAccessor CreateFieldAccessor(bool valueType, FieldInfo fieldInfo)
        {
            Type instanceType = valueType ? typeof(ValueMemberAccessor<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType)
                 : typeof(ObjectMemberAccessor<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);
            return (MemberAccessor)Activator.CreateInstance(instanceType, fieldInfo);
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
