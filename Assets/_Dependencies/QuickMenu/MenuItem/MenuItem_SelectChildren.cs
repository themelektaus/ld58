using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace QuickMenu
{
    public class MenuItem_SelectChildren : MenuItem
    {
        public override string title => "Select Children";

        public override bool Validation(Context context)
        {
            if (!context.transform)
                return false;

            foreach (Transform _ in context.transform)
                return true;

            return false;
        }

        public override bool Command(Context context)
        {
            var selection = new List<GameObject>();
            foreach (Transform transform in context.transform)
                selection.Add(transform.gameObject);
            Selection.objects = selection.ToArray();
            return true;
        }
    }
}