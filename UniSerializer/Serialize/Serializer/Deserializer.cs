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

        public virtual void SerializeObject<T>(ref T obj)
        {
            if(obj == null)
            {
                obj = (T)CreateObject();
            }

            Type type = obj.GetType();

            if (obj is ISerializable ser)
            {
                StartObject(type);
                ser.Serialize(this);
                EndObject();
            }
            else
            {
                if (type != typeof(T))
                {
                    FormatterCache.Get(type).Serialize(this, ref Unsafe.As<T, Object>(ref obj));
                }
                else
                    FormatterCache<T>.Instance.Serialize(this, ref obj);
            }

        }

        public virtual void SerializeProperty<T>(string name, ref T val)
        {
            if(StartProperty(name))
            {
                Serialize(ref val);

                EndProperty();
            }

        }

        public virtual bool StartObject(System.Type type)
        {
            return false;
        }

        public virtual void EndObject()
        {
        }

        public virtual bool StartArray(System.Type type, ref int len)
        {
            return false;
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

        protected virtual object CreateObject()
        {
            return default;
        }

        protected virtual void SerializePrimitive<T>(ref T val)
        {
        }

        protected virtual void SerializeBytes<T>(ref byte[] val)
        {
        }

    }
}
