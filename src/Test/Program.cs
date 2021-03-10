using System;
using System.Collections.Generic;

namespace UniSerializer
{
    public class RefObject
    {
        public string Name { get; set; }
        public int ID { get; set; }

    }

    public struct StructObject
    {
        public int X { get; set; }
        public int Y { get; set; }

        public StructObject[] StructObjects { get; set; }

        public RefObject RefObject { get; set; }

        public SerializeableObject SerializeableObject { get; set; }
    }


    public class ClassObject
    {
        float floatVal;
        int[] intArray;

        public StructObject Pos { get; set; }
        public Object NullObject { get; set; }

        public float FloatVal { get => floatVal; set => floatVal = value; }
        public int[] IntArray { get => intArray; set => intArray = value; }

        public List<float> FloatList { get; set; }
        public Dictionary<string, string> StringDict { get; set; }

        public List<ClassObject> Children { get; set; }

    }

    public class SerializeableObject : ISerializable
    {
        float floatVal;
        int[] intArray;

        public void Serialize(ISerializer serializer)
        {
            serializer.SerializeProperty("floatVal", ref floatVal);
            serializer.SerializeProperty("intArray", ref intArray);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Random r = new Random();

            var refObj1 = new RefObject
            {
                Name = "RefObject1", ID = 10001,
            };

            var refObj2 = new RefObject
            {
                Name = "RefObject2",
                ID = 10002,
            };


            var aa = new ClassObject();
            aa.Pos = new StructObject
            {
                X = 111, Y = 111,
                RefObject =  refObj1,
                StructObjects = new StructObject[]
                {
                    new StructObject
                    { 
                        X = 222, Y = 222,
                        RefObject =  refObj1,

                    },

                    new StructObject
                    {
                        X = 333, Y = 333,
                        RefObject =  refObj2,
                    },

                    new StructObject
                    {
                        X = 444, Y = 444,
                        RefObject =  refObj2,
                    }

                },
                SerializeableObject = new SerializeableObject()
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

            aa.Children = new List<ClassObject>();
            aa.Children.Add(new ClassObject());

            new JsonSerializer().Save((object)aa, "test.json");

            var obj = new JsonDeserializer().Load<ClassObject>("test.json");

            new JsonSerializer().Save((object)obj, "test1.json");

        }
    }
}
