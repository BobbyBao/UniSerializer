using System;
using System.Collections.Generic;
using System.Text;

namespace UniSerializer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SerializedFieldAttribute : Attribute
    {
        public SerializedFieldAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NoPolymorphicAttribute : Attribute
    {
        public NoPolymorphicAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterFormatterAttribute : Attribute
    {
        public Type FormatterType { get; private set; }
        public int Priority { get; private set; }

        public RegisterFormatterAttribute(Type formatterType, int priority = 0)
        {
            this.FormatterType = formatterType;
            this.Priority = priority;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterFormatterFactoryAttribute : Attribute
    {
        public Type FormatterFactoryType { get; private set; }
        public int Priority { get; private set; }

        public RegisterFormatterFactoryAttribute(Type formatterFactoryType, int priority = 0)
        {
            this.FormatterFactoryType = formatterFactoryType;
            this.Priority = priority;
        }
    }
}
