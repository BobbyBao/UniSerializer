using System;
using System.Collections.Generic;
using System.Text;

namespace UniSerializer
{
    [Flags]
    public enum SerializationMode
    {
        /// <summary>
        /// Only fields explicitly marked for serialization
        /// </summary>
        Explicit = 0,

        /// <summary>
        /// All the public properties with both setter and a getter
        /// </summary>
        Properties = 1,

        /// <summary>
        /// All the public and not readonly fields
        /// </summary>
        Fields = 2,

        /// <summary>
        /// All the members respecting either <see cref="Properties"/> or <see cref="Fields"/> criterias
        /// </summary>
        AllMembers = Properties | Fields,
    }


    public class SerializationConfig
    {
        public static Action<LogLevel, string> LogWritter
        {
            set => Log.LogWriter = value;
        }

        public static SerializationMode SerializationMode { get; set; } = SerializationMode.Properties;

        public static void Init()
        {
        }

        public static void Register<T>(Formatter<T> formatter)
        {
            FormatterCache<T>.Register(formatter);
        }

        public static void Register<T>(FormatFunc<T> formatter)
        {
            FormatterCache<T>.Register(new FormatterProxy<T>(formatter));
        }
    }
}
