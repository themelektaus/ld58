using System;
using UnityEngine;
using UnityEngine.UI;

namespace NodeEditor
{
    public class OpenGraphDialog : MonoBehaviour
    {
        public GraphList graphList;
        public Button cancelButton;

        public event Action onDestroy;

        public void Stop()
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}
