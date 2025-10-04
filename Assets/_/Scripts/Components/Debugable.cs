using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Debugable")]
    public class Debugable : MonoBehaviour
    {
        [SerializeField] List<DebugMessage> debugMessages = new();

        [Serializable]
        public struct DebugMessage
        {
            public string message;
            public DateTime timestamp;

            public DebugMessage(string message)
            {
                this.message = message;
                timestamp = DateTime.Now;
            }
        }

        public void AddDebugMessage(string message)
        {
            debugMessages.Add(new(message));
        }
    }
}
