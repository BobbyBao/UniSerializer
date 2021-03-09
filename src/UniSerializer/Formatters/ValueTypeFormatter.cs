using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UniSerializer
{
    public class ValueTypeFormatter<T> : Formatter<T> where T : new()
    {
        static MetaInfo memberMap = MetaInfo.Get<T>();

        public override void Serialize(ISerializer serializer, ref T obj)
        {
            serializer.StartObject(typeof(T));

            if (obj is ISerializable serializable)
            {
                serializable.Serialize(serializer);
            }
            else
            {
                foreach (var it in memberMap)
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
