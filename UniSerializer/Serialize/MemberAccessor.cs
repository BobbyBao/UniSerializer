using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UniSerializer
{
    public abstract class MemberAccessor
    {
        protected MemberInfo memberInfo;

        public object defaultVal;

        public string Name => memberInfo.Name;

        public virtual bool IsDefault(object val)
        {
            return false;
        }

        public abstract bool Get(object obj, out object value);
        public abstract bool Set(object obj, object value);

        public abstract void Serialize(ISerializer serializer, object obj);

    }


    public class ObjectMemberAccessor<K, T> : MemberAccessor
    {
        public Func<K, T> getter;
        public Action<K, T> setter;

        public ObjectMemberAccessor(FieldInfo fieldInfo)
        {
            this.memberInfo = fieldInfo;
            //getter = (Func<K, T>)Delegate.CreateDelegate(typeof(Func<K, T>), fieldInfo.GetMethod);
            //setter = (Action<K, T>)Delegate.CreateDelegate(typeof(Action<K, T>), fieldInfo.SetMethod);
        }

        public ObjectMemberAccessor(PropertyInfo propertyInfo)
        {
            this.memberInfo = propertyInfo;
            getter = (Func<K, T>)Delegate.CreateDelegate(typeof(Func<K, T>), propertyInfo.GetMethod);
            setter = (Action<K, T>)Delegate.CreateDelegate(typeof(Action<K, T>), propertyInfo.SetMethod);
        }

        public override bool Get(object obj, out object val)
        {
            val = getter((K)obj);
            return true;
        }

        public override bool Set(object obj, object val)
        {
            setter((K)obj, (T)val);
            return true;
        }

        public bool Get(object obj, out T val)
        {
            val = getter((K)obj);
            return true;
        }

        public bool Set(object obj, T val)
        {
            setter((K)obj, val);
            return true;
        }

        public override void Serialize(ISerializer serializer, object obj)
        {
            if(serializer.IsReading)
            {
                T val = default;
                serializer.Serialize(ref val);
                Set(obj, val);
            }
            else
            {
                if (Get(obj, out T val))
                {
                    serializer.Serialize(ref val);
                }
            }
        }

    }

    public class MemberAccessorMap<K> : Dictionary<string, MemberAccessor>
    {
        public MemberAccessorMap()
        {
            var properties = typeof(K).GetProperties(BindingFlags.Public | BindingFlags.Instance);
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

                Add(p.Name, CreatePropertySetterWrapper(p));
            }
        }

        public static MemberAccessor CreatePropertySetterWrapper(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            if (propertyInfo.CanWrite == false)
                throw new NotSupportedException("属性不支持写操作。");

            MethodInfo mi = propertyInfo.GetSetMethod(true);

            if (mi.GetParameters().Length > 1)
                throw new NotSupportedException("不支持构造索引器属性的委托。");

            Type instanceType = typeof(ObjectMemberAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (MemberAccessor)Activator.CreateInstance(instanceType, propertyInfo);
        }


    }

}
