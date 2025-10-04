using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    public abstract class Condition : MonoBehaviour
    {
        public abstract IEnumerable<bool> If();

        public enum LogicGate { And, Or }

        [SerializeField] protected LogicGate logicGate;

        [SerializeField] Object[] then;
        [SerializeField] Object[] @else;

        protected virtual void OnDisable()
        {
            SetTrue(false);
        }

        protected void Update()
        {
            SetTrue(IsTrue());
        }

        public bool IsTrue()
        {
            return logicGate switch
            {
                LogicGate.And => If().All(x => x),
                LogicGate.Or => If().Any(x => x),
                _ => false
            };
        }

        public void SetTrue(bool @true)
        {
            if (@true)
                Monitor.conditions.Add(this);
            else
                Monitor.conditions.Remove(this);

            foreach (var @object in then)
                @object.SetActiveOrEnable(@true);

            foreach (var @object in @else)
                @object.SetActiveOrEnable(!@true);
        }
    }
}
