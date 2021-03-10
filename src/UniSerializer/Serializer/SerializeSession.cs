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

        public bool GetRefObject(int id, out object obj)
        {
            if(id >= ojectList.Count)
            {
                obj = null;
                return false;
            }

            obj = ojectList[id];
            return true;
        }


    }
}
