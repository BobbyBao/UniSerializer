using System;
using System.Collections.Generic;

namespace UniSerializer
{
    public class AA
    {
        public float FloatVal { get => floatVal; set => floatVal = value; }
        public int[] IntArray { get => intArray; set => intArray = value; }
        float floatVal;

        int[] intArray = new int[10];

        public List<float> FloatList { get; set; } = new List<float> { 1.0f, 2.0f };


        public void Accept(Serializer visitor)
        {
            visitor.SerializeProperty("floatVal", ref floatVal);
            visitor.SerializeProperty("intArray", ref intArray);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var aa = new AA();

            new JsonSerializer().Save((object)aa, "test.json");

            var obj = new JsonDeserializer().Load<AA>("test.json");

            new JsonSerializer().Save((object)aa, "test1.json");

        }
    }
}
