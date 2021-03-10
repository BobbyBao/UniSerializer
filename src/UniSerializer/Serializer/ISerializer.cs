namespace UniSerializer
{

    public interface ISerializable
    {
        void Serialize(ISerializer serializer);
    }

    public interface ISerializer
    {
        bool IsReading { get; }
        SerializeSession SerializeSession { get; set; }
        void Serialize<T>(ref T val);
        void Serialize(ref object val);
        void SerializeProperty<T>(string name, ref T val);
        bool StartObject<T>(ref T obj);
        void EndObject();
        bool StartArray<T>(ref T array, ref int len);
        void SetElement(int index);
        void EndArray();
        bool StartProperty(string name);
        void EndProperty();
        void SerializeNull();
        void SerializePrimitive<T>(ref T val);
        void SerializeString(ref string val);
        void SerializeBytes(ref byte[] val);
    }
}
