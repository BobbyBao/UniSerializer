using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace UniSerializer
{
    public enum ValueType : byte
    {
        Null,
        True,
        False,
        Number,
        String,
        Array,
        Map,
        Object,
        Bin
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SerializeValue
    {
        [FieldOffset(0)]
        public double number;
        [FieldOffset(8)]
        public string str;
        [FieldOffset(8)]
        public object obj;
        [FieldOffset(8)]
        public List<SerializeValue> array;
        [FieldOffset(8)]
        public Dictionary<string, SerializeValue> map;
        [FieldOffset(8)]
        public byte[] bytes;
        [FieldOffset(16)]
        public readonly ValueType type;

        public static readonly SerializeValue Null = new SerializeValue(ValueType.Null);

        public bool IsArray => type == ValueType.Array;
        public bool IsMap => type == ValueType.Map;
        public bool IsObject => type == ValueType.Object;
        public int ElementCount => type == ValueType.Array ? array.Count : type == ValueType.Map ? map.Count : 0;

        public SerializeValue(ValueType valueType)
        {
            type = valueType;
            number = default;
            str = default;
            obj = null;
            array = null;
            map = null;
            bytes = null;
        }

        public SerializeValue(List<SerializeValue> arr)
        {
            type = ValueType.Array;
            number = default;
            this.str = default;
            obj = null;
            map = null;
            bytes = null;
            array = arr;
        }

        public SerializeValue(Dictionary<string, SerializeValue> map)
        {
            type = ValueType.Map;
            number = default;
            this.str = default;
            obj = null;
            array = null;
            bytes = null;
            this.map = map;
        }

        public SerializeValue(byte[] bin)
        {
            type = ValueType.Array;
            number = default;
            this.str = default;
            obj = null;
            map = null;
            array = null;
            bytes = bin;
        }

        public static explicit operator bool(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.True || value.type == ValueType.False);
            return value.type == ValueType.True;
        }

        public static explicit operator sbyte(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (sbyte)value.number;
        }

        public static explicit operator byte(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (byte)value.number;
        }

        public static explicit operator short(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (short)value.number;
        }

        public static explicit operator ushort(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (ushort)value.number;
        }

        public static explicit operator char(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (char)value.number;
        }

        public static explicit operator int(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (int)value.number;
        }

        public static explicit operator uint(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (uint)value.number;
        }

        public static explicit operator long(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (long)value.number;
        }

        public static explicit operator ulong(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (ulong)value.number;
        }

        public static explicit operator float(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return (float)value.number;
        }

        public static explicit operator double(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.Number);
            return value.number;
        }

        public static explicit operator string(in SerializeValue value)
        {
            Debug.Assert(value.type == ValueType.String || value.type == ValueType.Null);
            return value.str;
        }

        public static implicit operator SerializeValue(bool value) => new SerializeValue(value ? ValueType.True : ValueType.False);
        public static implicit operator SerializeValue(sbyte value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(byte value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(short value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(ushort value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(char value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(int value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(uint value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(long value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(ulong value) => new SerializeValue(ValueType.Number) { number = (long)value };
        public static implicit operator SerializeValue(float value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(double value) => new SerializeValue(ValueType.Number) { number = value };
        public static implicit operator SerializeValue(string value) => new SerializeValue(ValueType.String) { str = value };

        public SerializeValue this[int index]
        {
            get
            {
                Debug.Assert(type == ValueType.Array);
                return array[index];
            }
            set
            {
                Debug.Assert(type == ValueType.Array);
                array[index] = value;
            }
        }

        public SerializeValue this[string key]
        {
            get
            {
                Debug.Assert(type == ValueType.Map);
                return map[key];
            }
            set
            {
                Debug.Assert(type == ValueType.Map);
                map[key] = value;
            }
        }

        public void Add(in SerializeValue element)
        {
            Debug.Assert(type == ValueType.Array);
            array.Add(element);
        }

        public bool TryGet(string key, out SerializeValue element)
        {
            Debug.Assert(type == ValueType.Map);
            return map.TryGetValue(key, out element);
        }

        public void Add(string key, in SerializeValue element)
        {
            Debug.Assert(type == ValueType.Map);
            map.Add(key, element);
        }
    }

}
