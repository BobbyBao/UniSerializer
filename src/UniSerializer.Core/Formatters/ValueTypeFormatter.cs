using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public class ValueTypeFormatter<T> : Formatter<T> where T : new()
    {
        static MetaInfo metaInfo = MetaInfo.Get<T>();

        public override void Serialize(ISerializer serializer, ref T obj)
        {
            serializer.StartObject(ref obj);

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
