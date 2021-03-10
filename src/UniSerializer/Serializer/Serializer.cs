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
        public bool IsWriting => !IsReading;
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
                FormatterCache<T>.Instance.Serialize(this, ref val);
                
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
                FormatterCache.Get(type).Serialize(this, ref val);
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

        public void SetElement(int index)
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

    }
}
