using System.Runtime.CompilerServices;

namespace UniSerializer
{
    public class ObjectFormatter<T> : Formatter<T> where T : new()
    {
        static MetaInfo memberMap = MetaInfo.Get<T>();

        public override void Serialize(ISerializer serializer, ref T obj)
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
