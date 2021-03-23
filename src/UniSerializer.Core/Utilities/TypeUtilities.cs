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

        private static readonly Type GenericListInterface = typeof(IList<>);
        private static readonly Type GenericCollectionInterface = typeof(ICollection<>);
        public static Type[] GetArgumentsOfInheritedOpenGenericInterface(this Type candidateType, Type openGenericInterfaceType)
        {
            if ((openGenericInterfaceType == GenericListInterface || openGenericInterfaceType == GenericCollectionInterface) && candidateType.IsArray)
            {
                return new Type[] { candidateType.GetElementType() };
            }

            if (candidateType == openGenericInterfaceType)
                return candidateType.GetGenericArguments();

            if (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == openGenericInterfaceType)
                return candidateType.GetGenericArguments();

            var baseType = candidateType.BaseType;
            while (baseType != null)
            {
                if (!baseType.IsGenericType) continue;

                var result = baseType.GetArgumentsOfInheritedOpenGenericInterface(openGenericInterfaceType);

                if (result != null)
                    return result;

                baseType = baseType.BaseType;
            }

            return null;
        }

    }
}
