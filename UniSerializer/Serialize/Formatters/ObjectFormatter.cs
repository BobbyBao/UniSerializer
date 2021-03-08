using System.Runtime.CompilerServices;

namespace UniSerializer
{
    public class ObjectFormatter<T> : IFormatter<T>
    {
        static MemberAccessorMap<T> memberMap = new MemberAccessorMap<T>();

        public override void Serialize(ISerializer serializer, ref T obj)
        {
            serializer.StartObject(typeof(T));           

            foreach (var it in memberMap)
            {
                if (serializer.StartProperty(it.Key))
                {
                    it.Value.Serialize(serializer, ref Unsafe.As<T, object>(ref obj));
                    serializer.EndProperty();
                }
            }

            serializer.EndObject();
        }

    }

}
