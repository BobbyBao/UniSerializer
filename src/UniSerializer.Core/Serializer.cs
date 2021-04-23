using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public abstract class Serializer : ISerializer
    {
        public SerializeSession Session { get; } = new SerializeSession();
        public virtual bool IsReading { get; } = false;
        public virtual bool IsWriting { get; } = true;
        public bool IsInProperty { get; set; } = true;

        public void Save<T>(T obj, string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Save(obj, stream);
            }
        }

        public abstract void Save<T>(T obj, Stream stream);

        public virtual void Serialize<T>(ref T val, uint flags)
        {
            Type type = typeof(T);
            if (type.IsPrimitive)
            {
                SerializePrimitive(ref val);
            }
            else
            {
                if(val == null)
                {
                    SerializeNull();
                }
                else if (type == val.GetType())
                {
                    FormatterCache<T>.Instance.Serialize(this, ref val, flags);
                }
                else
                {
                    FormatterCache.Get(val.GetType()).Serialize(this, ref Unsafe.As<T, object>(ref val), flags);
                }
            }
        }

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
        public abstract void Serialize<T>(ref T val, int count) where T : unmanaged;
    }
}
