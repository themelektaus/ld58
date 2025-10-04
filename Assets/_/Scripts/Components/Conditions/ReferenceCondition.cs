using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_CONDITION + "Reference")]
    public class ReferenceCondition : Condition
    {
        [SerializeField] Reference[] references;

        public override IEnumerable<bool> If()
        {
            return references.Select(reference =>
            {
                try
                {
                    var result = reference.Get();
                    var boolResult = result is UnityEngine.Object @object
                        ? @object
                        : reference.Get<bool>(result);

                    if (reference.invert)
                        boolResult = !boolResult;

                    return boolResult;
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorGUIUtility.PingObject(gameObject);
#endif
                    Debug.LogWarning(ex, this);
                    return false;
                }
            });
        }
    }
}
