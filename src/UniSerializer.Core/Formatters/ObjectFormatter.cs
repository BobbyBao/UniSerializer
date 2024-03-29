﻿using System.Runtime.CompilerServices;

namespace UniSerializer
{
    public class ObjectFormatter<T> : Formatter<T> where T : new()
    {
        static readonly MetaInfo metaInfo = MetaInfo.Get<T>();

        public override void Serialize(ISerializer serializer, ref T obj, uint flags)
        {
            if(!serializer.StartObject(ref obj))
            {
                return;
            }

            if (obj == null)
            {                    
                obj = new T();
            }

            if(obj is ISerializable serializable)
            {
                serializable.Serialize(serializer);
            }
            else
            {
                foreach (var it in metaInfo)
                {
                    if (serializer.StartProperty(it.Key))
                    {
                        it.Value.Serialize(serializer, ref Unsafe.As<T, object>(ref obj));
                        serializer.EndProperty();
                    }
                }

            }

            serializer.EndObject();
        }

    }

    public class AbstractObjectFormatter<T> : Formatter<T>
    {
        static readonly MetaInfo metaInfo = MetaInfo.Get<T>();

        public override void Serialize(ISerializer serializer, ref T obj, uint flags)
        {
            if (!serializer.StartObject(ref obj))
            {
                return;
            }

            if (obj is ISerializable serializable)
            {
                serializable.Serialize(serializer);
            }
            else
            {
                foreach (var it in metaInfo)
                {
                    if (serializer.StartProperty(it.Key))
                    {
                        it.Value.Serialize(serializer, ref Unsafe.As<T, object>(ref obj));
                        serializer.EndProperty();
                    }
                }

            }

            serializer.EndObject();
        }

    }
}
