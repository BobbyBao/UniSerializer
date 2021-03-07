﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        public abstract bool Get(ref object obj, out object value);
        public abstract bool Set(ref object obj, object value);

        public abstract void Serialize(ISerializer serializer, ref object obj);

    }


    public class ObjectMemberAccessor<K, T> : MemberAccessor
    {
        public Func<K, T> getter;
        public Action<K, T> setter;

        public ObjectMemberAccessor(FieldInfo fieldInfo)
        {
            this.memberInfo = fieldInfo;
            getter = EmitUtilities.CreateInstanceGetter<K, T>(fieldInfo);
            setter = EmitUtilities.CreateInstanceSetter<K, T>(fieldInfo);
        }

        public ObjectMemberAccessor(PropertyInfo propertyInfo)
        {
            this.memberInfo = propertyInfo;
            getter = (Func<K, T>)Delegate.CreateDelegate(typeof(Func<K, T>), propertyInfo.GetMethod);
            setter = (Action<K, T>)Delegate.CreateDelegate(typeof(Action<K, T>), propertyInfo.SetMethod);
        }

        public override bool Get(ref object obj, out object val)
        {
            val = getter((K)obj);
            return true;
        }

        public override bool Set(ref object obj, object val)
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

        public override void Serialize(ISerializer serializer, ref object obj)
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

    public class ValueMemberAccessor<K, T> : MemberAccessor
    {
        public ValueGetter<K, T> getter;
        public ValueSetter<K, T> setter;

        public ValueMemberAccessor(FieldInfo fieldInfo)
        {
            this.memberInfo = fieldInfo;
            getter = EmitUtilities.CreateInstanceFieldGetter<K, T>(fieldInfo);
            setter = EmitUtilities.CreateInstanceFieldSetter<K, T>(fieldInfo);
        }

        public ValueMemberAccessor(PropertyInfo propertyInfo)
        {
            this.memberInfo = propertyInfo;
            getter = EmitUtilities.CreateInstancePropertyGetter<K, T>(propertyInfo);
            setter = EmitUtilities.CreateInstancePropertySetter<K, T>(propertyInfo);
        }

        public override bool Get(ref object obj, out object val)
        {
            val = getter(ref Unsafe.As<object, K>(ref obj));
            return true;
        }

        public override bool Set(ref object obj, object val)
        {
            setter(ref Unsafe.As<object, K>(ref obj), (T)val);
            return true;
        }

        public bool Get(ref object obj, out T val)
        {
            val = getter(ref Unsafe.As<object, K>(ref obj));
            return true;
        }

        public bool Set(ref object obj, T val)
        {
            setter(ref Unsafe.As<object, K>(ref obj), val);
            return true;
        }

        public override void Serialize(ISerializer serializer, ref object obj)
        {
            if (serializer.IsReading)
            {
                T val = default;
                serializer.Serialize(ref val);
                Set(ref obj, val);
            }
            else
            {
                if (Get(ref obj, out T val))
                {
                    serializer.Serialize(ref val);
                }
            }
        }

    }
    public class MemberAccessorMap<K> : Dictionary<string, MemberAccessor>
    {
        Type type = typeof(K);

        public MemberAccessorMap()
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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

                Add(p.Name, CreatePropertyAccessor(p));
            }
        }

        public static MemberAccessor CreatePropertyAccessor(PropertyInfo propertyInfo)
        {
            Type instanceType = typeof(ObjectMemberAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (MemberAccessor)Activator.CreateInstance(instanceType, propertyInfo);
        }


    }

}