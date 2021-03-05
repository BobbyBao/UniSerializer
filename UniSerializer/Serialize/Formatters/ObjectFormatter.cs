namespace UniSerializer
{
    public class ObjectFormatter<T> : IFormatter<T>
    {
        static MemberAccessorMap<T> memberMap = new MemberAccessorMap<T>();

        public override void Serialize(ISerializer serializer, ref T obj)
        {
            serializer.StartObject();

            foreach (var it in memberMap)
            {
                serializer.StartProperty(it.Key);
                it.Value.Serialize(serializer, obj);
                serializer.EndProperty();
            }

            serializer.EndObject();
        }

    }

}
