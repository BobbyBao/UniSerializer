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

        public abstract void Save<T>(T obj, Stream stream);

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


    }
}
