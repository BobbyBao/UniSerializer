using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniSerializer
{
    public enum LogLevel
    {
        None,
        Info,
        Warn,
        Error,
    }

    public class Log
    {
        internal static Action<LogLevel, string> LogWriter { get; set; } = WriteLog;
        internal static LogLevel LogLevel { get; set; }

        public static void Info(params string[] msg)
        {
            if (LogLevel >= LogLevel.Info)
                LogWriter?.Invoke(LogLevel.Info, string.Concat(msg));
        }

        public static void Warning(params string[] msg)
        {
            if (LogLevel >= LogLevel.Warn)
                LogWriter?.Invoke(LogLevel.Warn, string.Concat(msg));
        }

        public static void Error(params string[] msg)
        {
            if(LogLevel >= LogLevel.Error)
                LogWriter?.Invoke(LogLevel.Error, string.Concat(msg));
        }

        static void WriteLog(LogLevel logLevel, string msg)
        {
            Console.WriteLine($"[{logLevel}] {msg}");
        }
    }
}
