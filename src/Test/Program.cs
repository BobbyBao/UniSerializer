using System;
using System.Collections.Generic;

namespace UniSerializer
{
    public struct Vec2
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class AA
    {
        float floatVal;
        int[] intArray;

        public Vec2 Pos { get; set; }

        public float FloatVal { get => floatVal; set => floatVal = value; }
        public int[] IntArray { get => intArray; set => intArray = value; }

        public List<float> FloatList { get; set; }
        public Dictionary<string, string> StringDict { get; set; }

        public List<AA> Children { get; set; }

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
            Random r = new Random();

            var aa = new AA();
            aa.Pos = new Vec2
            {
                X = 111, Y = 222
            };

            aa.IntArray = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            aa.FloatList = new List<float>();

            for(int i = 0; i < 10; i++)
            {
                aa.FloatList.Add((float)r.NextDouble());
            }


            aa.StringDict = new Dictionary<string, string>
            {
                ["Key1"] = "Value1",
                ["Key2"] = "Value2",
                ["Key3"] = "Value3",
            };

            aa.Children = new List<AA>();
            aa.Children.Add(new AA());

            new JsonSerializer().Save((object)aa, "test.json");

            var obj = new JsonDeserializer().Load<AA>("test.json");

            new JsonSerializer().Save((object)obj, "test1.json");

        }
    }
}
