using System;
using System.Collections;
using System.Collections.Generic;

namespace UniSerializer
{
    public static class AotHelper
    {
        public static void Ensure(Action action)
        {
            if (IsFalse())
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("", e);
                }
            }
        }

        public static void EnsureType<T>() where T : new()
        {
            Ensure(() => new T());
        }

        public static void EnsureList<T>() where T : new()
        {
            Ensure(() =>
            {
                var a = new List<T>();                
                FormatterCache<T>.Register(new ObjectFormatter<T>());
                FormatterCache<T[]>.Register(new ArrayFormatter<T>());
                FormatterCache<List<T>>.Register(new ListFormatter<T>());
            });
        }

        public static void EnsureDictionary<TKey, TValue>()
        {
            Ensure(() =>
            {
                var a = new Dictionary<TKey, TValue>();
                FormatterCache<Dictionary<TKey, TValue>>.Register(new DictionaryFormatter<TKey, TValue>());
            });
        }

        private static bool s_alwaysFalse = DateTime.UtcNow.Year < 0;

        public static bool IsFalse()
        {
            return s_alwaysFalse;
        }
    }
}