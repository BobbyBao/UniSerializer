namespace UniSerializer
{

    public interface ISerializable
    {
        void Serialize(ISerializer serializer);
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
}
