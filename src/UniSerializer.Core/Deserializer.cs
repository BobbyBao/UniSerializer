using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public abstract class Deserializer : ISerializer
    {
        public SerializeSession Session { get; } = new SerializeSession();
        public virtual bool IsReading { get; } = true;
        public virtual bool IsWriting { get; } = false;
        public bool IsInProperty { get; set; } = true;

        public T Load<T>(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return Load<T>(stream);
            }
        }

        public abstract T Load<T>(Stream stream);        

        public virtual void Serialize<T>(ref T val, uint flags)
        {
            Type type = typeof(T);
            if (type.IsPrimitive)
            {
                SerializePrimitive(ref val);
            }
            else if(type == typeof(string))
            {
                SerializeString(ref Unsafe.As<T, string>(ref val));
            }
            else
            {
                SerializeObject(ref val, flags);
            }
        }

        private void SerializeObject<T>(ref T obj, uint flags)
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
                if(type == typeof(T))                
                    FormatterCache<T>.Instance.Serialize(this, ref obj, flags);                
                else
                    FormatterCache.Get(type).Serialize(this, ref Unsafe.As<T, object>(ref obj), flags);           
            }
            else
                FormatterCache<T>.Instance.Serialize(this, ref obj, flags);
        }

        protected abstract bool CreateObject(out object obj);      
        public abstract bool StartObject<T>(ref T obj);
        public abstract void EndObject();
        public abstract bool StartProperty(string name);
        public abstract void EndProperty();
        public abstract bool StartArray<T>(ref T array, ref int len);
        public abstract void SetElement(int index);
        public abstract void EndArray();
        public abstract void SerializeNull();
        public abstract void SerializePrimitive<T>(ref T val);
        public abstract void SerializeString(ref string val);
        public abstract void SerializeBytes(ref byte[] val);
        public abstract void SerializeGuid(ref Guid val);
        public abstract void SerializeUnmanaged<T>(ref T val, int count) where T : unmanaged;
        public abstract void SerializeMemory(ref IntPtr data, ref ulong length);
    }
}
