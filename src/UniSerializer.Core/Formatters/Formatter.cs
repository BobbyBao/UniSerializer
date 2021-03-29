using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UniSerializer
{
    public interface IFormatter
    {        
        void Serialize(ISerializer serialzer, ref object obj, uint flags);
    }
    
    public abstract class Formatter<T> : IFormatter
    {
        public static Formatter<T> Instance;

        public void Serialize(ISerializer serialzer, ref object obj, uint flags)
        {
            Serialize(serialzer, ref Unsafe.As<object, T>(ref obj), flags);
        }

        public abstract void Serialize(ISerializer serialzer, ref T obj, uint flags);       
    }

    public delegate void FormatFunc<T>(ISerializer serializer, ref T val);
    public class FormatterProxy<T> : Formatter<T>
    {
        FormatFunc<T> formatFunc;
        public FormatterProxy(FormatFunc<T> func)
        {
            formatFunc = func;
        }

        public override void Serialize(ISerializer serialzer, ref T val, uint flags)
        {
            formatFunc.Invoke(serialzer, ref val);
        }
    }

    public class StringFormatter : Formatter<string>
    {
        public override void Serialize(ISerializer serialzer, ref string val, uint flags)
        {
            serialzer.SerializeString(ref val);            
        }
    }

    public class GuidFormatter : Formatter<Guid>
    {
        public override void Serialize(ISerializer serialzer, ref Guid val, uint flags)
        {
            serialzer.Serialize(ref val);
        }
    }

    public class BytesFormatter : Formatter<byte[]>
    {
        public override void Serialize(ISerializer serialzer, ref byte[] val, uint flags)
        {
            serialzer.SerializeBytes(ref val);
        }
    }

    public class EnumFormatter<T> : Formatter<T> where T : struct
    {
        public override void Serialize(ISerializer serialzer, ref T val, uint flags)
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
        public override void Serialize(ISerializer serialzer, ref T val, uint flags)
        {
            serialzer.SerializePrimitive(ref val);
        }
    }
}
