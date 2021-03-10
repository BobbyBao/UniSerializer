using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public abstract class Deserializer : ISerializer
    {
        public bool IsReading { get; } = true;
        public bool IsWriting => !IsReading;

        public SerializeSession SerializeSession { get; set; }

        public virtual void Serialize<T>(ref T val)
        {
            Type type = typeof(T);
            if (type.IsPrimitive)
            {
                SerializePrimitive(ref val);
            }
            else
            {
                SerializeObject(ref val);
            }
        }

        public virtual void Serialize(ref object val)
        {
            Type type = val.GetType();

            if (type.IsPrimitive)
            {
                SerializePrimitive(ref val);
            }
            else
            {
                SerializeObject(ref val);
            }
        }

        private void SerializeObject<T>(ref T obj)
        {
            if(obj == null)
            {
                obj = (T)CreateObject();
            }

            if(obj != null)
            {
                Type type = obj.GetType();                    
                FormatterCache.Get(type).Serialize(this, ref Unsafe.As<T, Object>(ref obj));           
            }
            else
                FormatterCache<T>.Instance.Serialize(this, ref obj);
        }

        public virtual void SerializeProperty<T>(string name, ref T val)
        {
            if(StartProperty(name))
            {
                Serialize(ref val);

                EndProperty();
            }

        }

        public virtual bool StartObject<T>(ref T obj)
        {
            return false;
        }

        public virtual void EndObject()
        {
        }

        public virtual bool StartArray<T>(ref T array, ref int len)
        {
            return false;
        }

        public virtual void SetElement(int index)
        {
        }

        public virtual void EndArray()
        {
        }

        public virtual bool StartProperty(string name)
        {
            return false;
        }

        public virtual void EndProperty()
        {
        }

        public virtual void SerializeNull()
        {
        }

        public virtual void SerializePrimitive<T>(ref T val)
        {
        }

        public virtual void SerializeString(ref string val)
        {
        }

        public virtual void SerializeBytes(ref byte[] val)
        {
        }

        protected virtual object CreateObject()
        {
            return default;
        }

    }
}
