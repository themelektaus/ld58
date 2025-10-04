using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NodeEditor
{
    public class Node : MonoBehaviour
    {
        [Serializable]
        public class Data
        {
            public long id;
            public string type;
            public Vector2 position;


            
            public abstract class Variable<T>
            {
                public string name;
                public abstract T[] values { get; set; }
            }


            
            [Serializable]
            public class BooleanVariable : Variable<bool>
            {
                public bool[] data;
                public override bool[] values { get => data; set => data = value; }
            }
            
            [SerializeField] List<BooleanVariable> booleanVariables = new();
            
            public bool GetBoolean(string name) => Get<BooleanVariable, bool>(booleanVariables, name);
            public bool GetBoolean(string name, bool defaultValue)
            {
                Set(booleanVariables, name, defaultValue, false);
                return GetBoolean(name);
            }
            public bool[] GetBooleans(string name) => GetAll<BooleanVariable, bool>(booleanVariables, name);
            public void SetBoolean(string name, bool value) => Set(booleanVariables, name, value, true);
            public void SetBooleans(string name, bool[] values) => SetAll(booleanVariables, name, values, true);



            [Serializable]
            public class IntegerVariable : Variable<int>
            {
                public int[] data;
                public override int[] values { get => data; set => data = value; }
            }

            [SerializeField] List<IntegerVariable> integerVariables = new();
            
            public int GetInteger(string name) => Get<IntegerVariable, int>(integerVariables, name);
            public int GetInteger(string name, int defaultValue)
            {
                Set(integerVariables, name, defaultValue, false);
                return GetInteger(name);
            }
            public int[] GetIntegers(string name) => GetAll<IntegerVariable, int>(integerVariables, name);
            public void SetInteger(string name, int value) => Set(integerVariables, name, value, true);
            public void SetIntegers(string name, int[] values) => SetAll(integerVariables, name, values, true);


            
            [Serializable]
            public class FloatVariable : Variable<float>
            {
                public float[] data;
                public override float[] values { get => data; set => data = value; }
            }

            [SerializeField] List<FloatVariable> floatVariables = new();

            public float GetFloat(string name) => Get<FloatVariable, float>(floatVariables, name);
            public float GetFloat(string name, float defaultValue)
            {
                Set(floatVariables, name, defaultValue, false);
                return GetFloat(name);
            }
            public float[] GetFloats(string name) => GetAll<FloatVariable, float>(floatVariables, name);
            public void SetFloat(string name, float value) => Set(floatVariables, name, value, true);
            public void SetFloats(string name, float[] values) => SetAll(floatVariables, name, values, true);



            [Serializable]
            public class StringVariable : Variable<string>
            {
                public string[] data;
                public override string[] values { get => data; set => data = value; }
            }

            [SerializeField] List<StringVariable> stringVariables = new();

            public string GetString(string name) => Get<StringVariable, string>(stringVariables, name);
            public string[] GetStrings(string name) => GetAll<StringVariable, string>(stringVariables, name);
            public void SetString(string name, string value) => Set(stringVariables, name, value, true);
            public void SetStrings(string name, string[] values) => SetAll(stringVariables, name, values, true);



            // MyTODO: Object Variable

            //[Serializable]
            //public class ObjectVariable : Variable<UnityEngine.Object>
            //{
            //    public UnityEngine.Object[] data;
            //    public override UnityEngine.Object[] values { get => data; set => data = value; }
            //}

            //[SerializeField] List<ObjectVariable> objectVariables = new();

            //public UnityEngine.Object GetObject(string name) => Get<ObjectVariable, UnityEngine.Object>(objectVariables, name);
            //public UnityEngine.Object[] GetObjects(string name) => GetAll<ObjectVariable, UnityEngine.Object>(objectVariables, name);
            //public void SetObject(string name, UnityEngine.Object value) => Set(objectVariables, name, value, true);
            //public void SetObjects(string name, UnityEngine.Object[] values) => SetAll(objectVariables, name, values, true);



            static TValue Get<TVariable, TValue>(IList<TVariable> variables, string name) where TVariable : Variable<TValue>
            {
                var values = GetAll<TVariable, TValue>(variables, name);
                return values.Length == 0 ? default : values[0];
            }

            static TValue[] GetAll<TVariable, TValue>(IList<TVariable> variables, string name) where TVariable : Variable<TValue>
            {
                var variable = variables.FirstOrDefault(x => x.name == name);
                return variable?.values ?? new TValue[0];
            }

            static void Set<TVariable, TValue>(IList<TVariable> variables, string name, TValue value, bool force) where TVariable : Variable<TValue>, new()
            {
                SetAll(variables, name, new[] { value }, force);
            }

            static void SetAll<TVariable, TValue>(IList<TVariable> variables, string name, TValue[] values, bool force) where TVariable : Variable<TValue>, new()
            {
                var variable = variables.FirstOrDefault(x => x.name == name);
                if (variable is null)
                    variables.Add(new() { name = name, values = values });
                else if (force)
                    variable.values = values;
            }


            
            [NonSerialized] public NodeEditor nodeEditor;
            [NonSerialized] public Node node;

            
            
            public Data Transition(GraphData graphData, int number)
            {
                if (number == 0)
                {
                    return this;
                }

                if (number < 0)
                {
                    return graphData.connectionDataList
                        .Where(x => x.targetNodeDataId == id)
                        .Select(x => graphData.nodeDataList.FirstOrDefault(y => y.id == x.sourceNodeDataId))
                        .OrderByDescending(x => x.position.y)
                        .Skip(-number - 1)
                        .FirstOrDefault() ?? this;
                }

                return graphData.connectionDataList
                    .Where(x => x.sourceNodeDataId == id)
                    .Select(x => graphData.nodeDataList.FirstOrDefault(y => y.id == x.targetNodeDataId))
                    .OrderByDescending(x => x.position.y)
                    .Skip(number - 1)
                    .FirstOrDefault() ?? this;
            }

            public int GetInCount(GraphData graphData)
            {
                return graphData.connectionDataList.Count(x => x.targetNodeDataId == id);
            }

            public int GetOutCount(GraphData graphData)
            {
                return graphData.connectionDataList.Count(x => x.sourceNodeDataId == id);
            }
        }

        public Data data { get; private set; }

        public string type;

        [SerializeField] RectTransform inPointTransform;
        [SerializeField] RectTransform outPointTransform;
        [SerializeField] GameObject selectionBorder;
        
        public event Action onRead;
        public event Action onWrite;

        public Vector3 inPoint => inPointTransform.position;
        public Vector3 outPoint => outPointTransform.position;

        public bool selected => selectionBorder.activeSelf;
        
        public WindowDragger dragger { get; private set; }

        void Awake()
        {
            dragger = GetComponentInChildren<WindowDragger>();
        }

        public void Read(Data data)
        {
            this.data = data;
            transform.localPosition = data.position;
            onRead?.Invoke();
        }

        public void Write()
        {
            data.position = transform.localPosition;
            onWrite?.Invoke();
        }

        public void Connect()
        {
            data.nodeEditor.ConnectNode(data.id);
        }

        public void Remove()
        {
            data.nodeEditor.RemoveNode(data.id);
        }

        public void CompleteConnection(BaseEventData e)
        {
            if (e is PointerEventData pe && pe.button == 0)
            {
                data.nodeEditor.CompleteConnection(data.id);
                return;
            }

            data.nodeEditor.CancelConnection();
        }

        public void Select()
        {
            selectionBorder.gameObject.SetActive(true);
        }

        public void Deselect()
        {
            selectionBorder.gameObject.SetActive(false);
        }
    }
}