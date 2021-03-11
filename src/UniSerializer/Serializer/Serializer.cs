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
        public bool IsReading { get; } = false;
        public SerializeSession Session { get; } = new SerializeSession();

        public void Save<T>(T obj, string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Save(obj, stream);
            }
        }

        public virtual void Save<T>(T obj, Stream stream)
        {
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
                if (val == null || type == val.GetType())
                {
                    FormatterCache<T>.Instance.Serialize(this, ref val);
                }
                else
                {
                    FormatterCache.Get(val.GetType()).Serialize(this, ref Unsafe.As<T, object>(ref val));
                }
            }
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

        public void SetElement(int index)
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

    }
}
