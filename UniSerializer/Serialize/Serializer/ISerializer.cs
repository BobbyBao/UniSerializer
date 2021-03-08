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
        void SerializeNull();
        void SerializeProperty<T>(string name, ref T val);
        bool StartObject(System.Type type);
        void EndObject();
        bool StartArray(System.Type type, ref int len);
        void EndArray();
        bool StartProperty(string name);
        void EndProperty();
    }
}
