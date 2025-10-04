using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor
{
    public class InputFieldList : MonoBehaviour
    {
        [SerializeField] RectTransform listTransform;
        [SerializeField] CustomInputField customInputField;
        [SerializeField] List<string> values;

        public string[] Get() => values.ToArray();

        public void Set(params string[] values)
        {
            this.values.Clear();
            this.values.AddRange(values);
            Refresh();
        }

        public void Clear()
        {
            values.Clear();
            Refresh();
        }

        public void AddItem()
        {
            values.Add(string.Empty);
            Refresh();
        }

        public void RemoveLastItem()
        {
            if (values.Count == 0)
                return;
            
            values.RemoveAt(values.Count - 1);
            Refresh();
        }

        void Refresh()
        {
            foreach (Transform child in listTransform)
                Destroy(child.gameObject);
            
            for (int i = 0; i < values.Count; i++)
            {
                int _i = i;
                if (Instantiate(customInputField.gameObject, listTransform).TryGetComponent(out CustomInputField _customInputField))
                {
                    _customInputField.SetText(values[_i]);
                    _customInputField.inputText.onValueChanged.AddListener(value => values[_i] = _customInputField.inputText.text);
                }
            }
        }
    }
}