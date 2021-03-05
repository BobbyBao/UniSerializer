﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        void StartProperty(string name);
        void EndProperty();
    }

    public abstract class Serializer : ISerializer
    {
        public bool IsReading { get; }
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
            if (obj is ISerializable ser)
            {
                StartObject();
                ser.Serialize(this);
                EndObject();
            }
            else
            {
                Type type = obj.GetType();
                if (type != typeof(T))
                {
                    FormatterCache.Get(type).Serialize(this, ref Unsafe.As<T, Object>(ref obj));
                }
                else
                    FormatterCache<T>.instance.Serialize(this, ref obj);
            }

        }

        public virtual void SerializeProperty<T>(string name, ref T val)
        {
            StartProperty(name);

            Serialize(ref val);

            EndProperty();
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

        public virtual void StartProperty(string name)
        {
        }

        public virtual void EndProperty()
        {
        }

        public virtual void SerializeNull()
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