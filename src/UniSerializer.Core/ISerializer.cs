﻿using System;

namespace UniSerializer
{

    public interface ISerializable
    {
        void Serialize(ISerializer serializer);
    }

    public interface ISerializer
    {
        SerializeSession Session { get; }
        bool IsReading { get; }
        bool IsWriting { get; }
        bool IsInProperty { get; }

        void Serialize<T>(ref T val, uint flags = 0);
        bool StartObject<T>(ref T obj);
        void EndObject();
        bool StartProperty(string name);
        void EndProperty();
        bool StartArray<T>(ref T array, ref int len);
        void SetElement(int index);
        void EndArray();
        void SerializeNull();
        void SerializePrimitive<T>(ref T val);
        void SerializeString(ref string val);
        void SerializeBytes(ref byte[] val);
        void SerializeGuid(ref Guid val);
        void SerializeUnmanaged<T>(ref T val, int count) where T : unmanaged;
        void SerializeMemory(ref IntPtr data, ref ulong length);
    }

    public static class SerializerExt
    {
        public static void SerializeProperty<T>(this ISerializer serializer, string name, ref T val, uint flags = 0)
        {
            if (serializer.StartProperty(name))
            {
                serializer.Serialize(ref val, flags);

                serializer.EndProperty();
            }

        }
    }
}
