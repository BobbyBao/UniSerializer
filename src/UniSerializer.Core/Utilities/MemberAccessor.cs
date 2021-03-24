using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public abstract class MemberAccessor
    {
        protected MemberInfo memberInfo;
        protected byte[] utf8Name;
        public byte[] UTF8Name => utf8Name;
        public string Name => memberInfo.Name;

        public virtual bool Get(ref object obj, out object value) { value = default; return false; }
        public virtual bool Set(ref object obj, object value) { return false; }

        public abstract void Serialize(ISerializer serializer, ref object obj);
    }

    public class ObjectMemberAccessor<K, T> : MemberAccessor
    {
        private Func<K, T> getter;
        private Action<K, T> setter;

        public ObjectMemberAccessor(FieldInfo fieldInfo)
        {
            memberInfo = fieldInfo;
            utf8Name = Encoding.UTF8.GetBytes(fieldInfo.Name);
            getter = EmitUtilities.CreateInstanceGetter<K, T>(fieldInfo);
            setter = EmitUtilities.CreateInstanceSetter<K, T>(fieldInfo);
        }

        public ObjectMemberAccessor(PropertyInfo propertyInfo)
        {
            memberInfo = propertyInfo;
            utf8Name = Encoding.UTF8.GetBytes(propertyInfo.Name);
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
                serializer.Serialize(ref val, 0);
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
        private ValueGetter<K, T> getter;
        private ValueSetter<K, T> setter;

        public ValueMemberAccessor(FieldInfo fieldInfo)
        {
            this.memberInfo = fieldInfo;
            getter = EmitUtilities.CreateInstanceFieldGetter<K, T>(fieldInfo);
            setter = EmitUtilities.CreateInstanceFieldSetter<K, T>(fieldInfo);
        }

        public ValueMemberAccessor(PropertyInfo propertyInfo)
        {
            this.memberInfo = propertyInfo;
            getter = (ValueGetter<K, T>)Delegate.CreateDelegate(typeof(ValueGetter<K, T>), propertyInfo.GetMethod);
            setter = (ValueSetter<K, T>)Delegate.CreateDelegate(typeof(ValueSetter<K, T>), propertyInfo.SetMethod);
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

}
