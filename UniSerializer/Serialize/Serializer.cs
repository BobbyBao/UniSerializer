using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UniSerializer
{

    public interface ISerializable
    {
        void Serialize(Serializer serializer);
    }

    public interface ISerializer
    {
        bool IsReading { get; }
        void Serialize<T>(ref T val);
        void Serialize(ref object val);
        void SerializeObject<T>(ref T val);
        void SerializeNull();
        void SerializeProperty<T>(string name, ref T val);
        void StartObject();
        void EndObject();
        void StartArray(ref int len);
        void EndArray();
        void StartAttribute(string name);
        void EndAttribute();
    }

    public abstract class Serializer : ISerializer
    {
        public bool IsReading { get; }
        public bool IsWriting => !IsReading;

        public virtual void Serialize<T>(ref T val)
        {
        }

        public virtual void Serialize(ref object val)
        {
        }

        public virtual void SerializeObject<T>(ref T val)
        {
        }

        public virtual void SerializeNull()
        {
        }

        public virtual void SerializeProperty<T>(string name, ref T val)
        {
        }

        public virtual void StartObject()
        {
        }

        public virtual void EndObject()
        {
        }

        public virtual void StartArray(ref int len)
        {
        }

        public virtual void EndArray()
        {
        }

        public virtual void StartAttribute(string name)
        {
        }

        public virtual void EndAttribute()
        {
        }

        protected virtual void SerializePrimitive<T>(ref T val)
        {
        }

        protected virtual void SerializeBytes<T>(ref byte[] val)
        {
        }

    }
}
