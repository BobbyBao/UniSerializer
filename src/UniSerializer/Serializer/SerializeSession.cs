using System;
using System.Collections.Generic;
using System.Text;

namespace UniSerializer
{
    public class SerializeSession
    {
        private List<object> ojectList = new List<object>();
        private Dictionary<object, int> object2ID = new Dictionary<object, int>();

        public int AddRefObject(object obj)
        {
            int id = ojectList.Count;
            ojectList.Add(obj);
            object2ID.Add(obj, id);
            return id;
        }

        public bool GetRef(object obj, out int id)
        {
            return object2ID.TryGetValue(obj, out id);
        }

        public object GetRefObject(int id)
        {
            if(id >= ojectList.Count)
            {
                return null;
            }

            return ojectList[id];            
        }


    }
}
