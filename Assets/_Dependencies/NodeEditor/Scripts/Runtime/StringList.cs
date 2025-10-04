using System;
using System.Collections.Generic;

namespace NodeEditor
{
    [Serializable]
    public class StringList
    {
        public List<string> items = new();

        public string AddGuid()
        {
            string guid;

            do guid = Guid.NewGuid().ToString();
            while (items.Contains(guid));

            items.Add(guid);

            return guid;
        }
    }
}