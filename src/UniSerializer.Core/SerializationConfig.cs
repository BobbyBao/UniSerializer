using System;
using System.Collections.Generic;
using System.Text;

namespace UniSerializer
{
    public class SerializationConfig
    {
        public static Action<LogLevel, string> LogWritter 
        { 
            set => Log.LogWriter = value;
        }

        public static void Init()
        {

            //FormatterCache.Register(typeof(T), this);
        }

    }
}
