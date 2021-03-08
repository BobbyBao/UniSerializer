using System.Runtime.CompilerServices;

namespace UniSerializer
{
    public interface IFormatter
    {        
        void Serialize(ISerializer visitor, ref object obj);
    }


    public class Formatter<T> : IFormatter
    {
        public void Serialize(ISerializer visitor, ref object obj)
        {
            Serialize(visitor, ref Unsafe.As<object, T>(ref obj));
        }

        public virtual void Serialize(ISerializer visitor, ref T obj)
        {
        }

    }

    public class StringFormatter : Formatter<string>
    {
        public override void Serialize(ISerializer visitor, ref string val)
        {              
            visitor.SerializeString(ref val);            
        }
    }

    public class EnumFormatter<T> : Formatter<T> where T : struct
    {
        public override void Serialize(ISerializer visitor, ref T val)
        {
            if(visitor.IsReading)
            {
                string str = default;
                visitor.SerializeString(ref str);
                val = System.Enum.Parse<T>(str);
            }
            else
            {
                string str = val.ToString();
                visitor.SerializeString(ref str);
            }

        }
    }

    public class PrimitiveFormatter<T> : Formatter<T>
    {
        public override void Serialize(ISerializer visitor, ref T val)
        {
            visitor.SerializePrimitive(ref val);
        }
    }
}
