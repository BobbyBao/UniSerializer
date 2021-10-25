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
        public Type Type { get; }
        public string TypeName => Type.Name;
        private Func<object> Factory { get; set; }

        public MetaInfo(Type type)
        {
            this.Type = type;

            if((SerializationConfig.SerializationMode & SerializationMode.Properties) != 0)
            {
                AddProperties();
            }
            
            if((SerializationConfig.SerializationMode & SerializationMode.Fields) != 0 && EmitUtilities.CanEmit)
            {
                AddFields();
            }
        }

        public object CreateInstance()
        {
            if(Factory != null)
                return Factory.Invoke();

            return Activator.CreateInstance(Type);
        }

        private void AddProperties()
        {
            var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetMethod == null)
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

                if(propertyInfo.SetMethod == null)
                {
                    if(propertyInfo.PropertyType.IsByRef && propertyInfo.IsDefined(typeof(SerializedFieldAttribute)))
                    {
                        var elementType = propertyInfo.PropertyType.GetElementType();
                        Type instanceType = typeof(RefMemberAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, elementType);
                        Add(propertyInfo.Name, (MemberAccessor)Activator.CreateInstance(instanceType, propertyInfo));                        
                    }
                    else
                    {
                        continue;
                    }
                }

                else
                {
                    Add(propertyInfo.Name, CreatePropertyAccessor(Type.IsValueType, propertyInfo));
                }

            }

        }

        private void AddFields()
        {
            var fields = Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

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

                Add(fieldInfo.Name, CreateFieldAccessor(Type.IsValueType, fieldInfo));

            }

        }

        static ConcurrentDictionary<Type, MetaInfo> metaInfoDB = new ConcurrentDictionary<Type, MetaInfo>();

        public static MetaInfo Get<T>() => Get(typeof(T));

        public static MetaInfo Get(Type type)
        {
            if (!metaInfoDB.TryGetValue(type, out var metaInfo))
            {
                metaInfo = new MetaInfo(type);
                metaInfoDB.TryAdd(type, metaInfo);
            }

            return metaInfo;
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


    }

}
