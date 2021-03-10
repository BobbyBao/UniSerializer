using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UniSerializer
{
    public static class TypeUtilities
    {
        static Dictionary<string, Type> nameToType = new Dictionary<string, Type>();
        static TypeUtilities()
        {
        }

        public static Type GetType(string name)
        {
            if(nameToType.TryGetValue(name, out var type))
            {
                return type;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in assemblies)
            {   
                var t = ass.GetType(name);
                if(t != null)
                {
                    nameToType.Add(name, t);
                    return t;
                }
            }

            return null;

        }

    }
}
