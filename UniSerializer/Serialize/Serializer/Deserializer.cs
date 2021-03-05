using System;
using System.Collections.Generic;
using System.Text;

namespace UniSerializer
{
    public abstract class Deserializer : ISerializer
    {
        public bool IsReading => throw new NotImplementedException();

        public void EndArray()
        {
            throw new NotImplementedException();
        }

        public void EndObject()
        {
            throw new NotImplementedException();
        }

        public void EndProperty()
        {
            throw new NotImplementedException();
        }

        public void Serialize<T>(ref T val)
        {
            throw new NotImplementedException();
        }

        public void Serialize(ref object val)
        {
            throw new NotImplementedException();
        }

        public void SerializeNull()
        {
            throw new NotImplementedException();
        }

        public void SerializeObject<T>(ref T val)
        {
            throw new NotImplementedException();
        }

        public void SerializeProperty<T>(string name, ref T val)
        {
            throw new NotImplementedException();
        }

        public void StartArray(ref int len)
        {
            throw new NotImplementedException();
        }

        public void StartObject()
        {
            throw new NotImplementedException();
        }

        public void StartProperty(string name)
        {
            throw new NotImplementedException();
        }
    }
}
