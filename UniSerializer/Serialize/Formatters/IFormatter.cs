using System.Runtime.CompilerServices;

namespace UniSerializer
{
    public interface IFormatter
    {        
        void Serialize(ISerializer visitor, ref object obj);
    }


    public class IFormatter<T> : IFormatter
    {
        public void Serialize(ISerializer visitor, ref object obj)
        {
            Serialize(visitor, ref Unsafe.As<object, T>(ref obj));
        }

        public virtual void Serialize(ISerializer visitor, ref T obj)
        {
        }

    }


}
