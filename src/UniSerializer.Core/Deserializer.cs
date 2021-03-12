using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public abstract class Deserializer : ISerializer
    {
        public bool IsReading { get; } = true;
        public SerializeSession Session { get; } = new SerializeSession();

        public T Load<T>(string path) where T : new()
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return Load<T>(stream);
            }
        }

        public virtual T Load<T>(Stream stream)
        {
            return default;
        }

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

        private void SerializeObject<T>(ref T obj)
        {
            if(obj == null)
            {
                if(!CreateObject(out var newObj))
                {
                    obj = (T)newObj;
                    return;
                }

                obj = (T)newObj;
            }

            if(obj != null)
            {
                Type type = obj.GetType();                    
                FormatterCache.Get(type).Serialize(this, ref Unsafe.As<T, object>(ref obj));           
            }
            else
                FormatterCache<T>.Instance.Serialize(this, ref obj);
        }

        public virtual bool StartObject<T>(ref T obj)
        {
            return false;
        }

        public virtual void EndObject()
        {
        }

        public virtual bool StartProperty(string name)
        {
            return false;
        }

        public virtual void EndProperty()
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

        protected virtual bool CreateObject(out object obj)
        {
            obj = default;
            return true;
        }

    }
}
