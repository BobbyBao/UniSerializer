using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UniSerializer
{
    public enum ValueType
    {
        Null,
        Bool,
        Number,
        String,
        Array,
        Object
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SerializeValue
    {
        [FieldOffset(0)]
        bool boolVal;
        [FieldOffset(0)]
        long number;
        [FieldOffset(0)]
        double dNumber;
        [FieldOffset(8)]
        string str;
        [FieldOffset(8)]
        SerializeValue[] array;
        [FieldOffset(8)]
        Dictionary<string, SerializeValue> obj;
        [FieldOffset(16)]
        ValueType type;

        public SerializeValue(int i)
        {
            type = ValueType.Number;
            boolVal = default;
            dNumber = default;
            str = default;
            array = null;
            obj = null;
            number = i;
        }

        public SerializeValue(double i)
        {
            type = ValueType.Number;
            boolVal = default;
            number = default;
            str = default;
            array = null;
            obj = null;
            dNumber = i;
        }

        public SerializeValue(string str)
        {
            type = ValueType.String;
            boolVal = default;
            dNumber = default;
            number = 0;
            array = null;
            obj = null;
            this.str = str;
        }
    }

}
