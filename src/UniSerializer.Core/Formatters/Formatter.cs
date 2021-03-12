using System.Runtime.CompilerServices;

namespace UniSerializer
{
    public interface IFormatter
    {        
        void Serialize(ISerializer serialzer, ref object obj);
    }


    public abstract class Formatter<T> : IFormatter
    {
        public void Serialize(ISerializer serialzer, ref object obj)
        {
            Serialize(serialzer, ref Unsafe.As<object, T>(ref obj));
        }

        public abstract void Serialize(ISerializer serialzer, ref T obj);       
    }

    public class StringFormatter : Formatter<string>
    {
        public override void Serialize(ISerializer serialzer, ref string val)
        {
            serialzer.SerializeString(ref val);            
        }
    }

    public class EnumFormatter<T> : Formatter<T> where T : struct
    {
        public override void Serialize(ISerializer serialzer, ref T val)
        {
            if(serialzer.IsReading)
            {
                string str = default;
                serialzer.SerializeString(ref str);                
                if(!System.Enum.TryParse(str, out val))
                {
                    Log.Error($"Cannot pars {str}, Enum type : {typeof(T)}");
                }
            }
            else
            {
                string str = val.ToString();
                serialzer.SerializeString(ref str);
            }

        }
    }

    public class PrimitiveFormatter<T> : Formatter<T>
    {
        public override void Serialize(ISerializer serialzer, ref T val)
        {
            serialzer.SerializePrimitive(ref val);
        }
    }
}
