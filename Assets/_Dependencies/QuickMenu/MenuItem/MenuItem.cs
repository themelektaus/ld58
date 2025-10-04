using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using Object = UnityEngine.Object;

namespace QuickMenu
{
    public abstract class MenuItem : IMenuItem
    {
        [AddQuickMenuItems]
        static IEnumerable<IMenuItem> _AddQuickMenuItems()
        {
            var types = TypeCache.GetTypesDerivedFrom<MenuItem>();
            foreach (var type in types.Where(x => !x.IsAbstract))
                yield return Activator.CreateInstance(type) as IMenuItem;
        }

        public virtual bool visible => true;

        public virtual int priority => 0;

        public abstract string title { get; }

        public virtual string description { get; }

        public virtual string category => null;
        public virtual string subCategory => null;
        
        public virtual bool Validation(Context context) =>
            !context.isAnimatorControllerTool;

        public abstract bool Command(Context context);

        public IEnumerable<VisualElement> GetParameterFields()
        {
            var fieldInfos = GetType().GetFields();

            foreach (var fieldInfo in fieldInfos)
            {
                VisualElement field;

                var label = fieldInfo.Name.SplitCamelCase();

                if (fieldInfo.FieldType == typeof(bool))
                    field = CreateBooleanField(label, fieldInfo);

                else if (fieldInfo.FieldType == typeof(int))
                    field = CreateIntegerField(label, fieldInfo);

                else if (fieldInfo.FieldType == typeof(float))
                    field = CreateFloatField(label, fieldInfo);

                else if (fieldInfo.FieldType == typeof(string))
                    field = CreateTextField(label, fieldInfo);

                // MyTODO: Support objects
                //         because at the moment Unity's Object Selector
                //         is immetiatelly closing
                else if (fieldInfo.FieldType == typeof(Object))
                    field = CreateObjectField(label, fieldInfo);

                else
                    continue;

                yield return field;
            }
        }

        BaseField<bool> CreateBooleanField(string label, FieldInfo fieldInfo)
        {
            var field = new Toggle
            {
                label = label,
                value = (bool) fieldInfo.GetValue(this)
            };
            field.RegisterValueChangedCallback(e => fieldInfo.SetValue(this, e.newValue));
            return field;
        }

        BaseField<int> CreateIntegerField(string label, FieldInfo fieldInfo)
        {
            var field = new IntegerField
            {
                label = label,
                value = (int) fieldInfo.GetValue(this),
            };
            field.RegisterValueChangedCallback(e => fieldInfo.SetValue(this, e.newValue));
            return field;
        }

        BaseField<float> CreateFloatField(string label, FieldInfo fieldInfo)
        {
            var field = new FloatField
            {
                label = label,
                value = (float) fieldInfo.GetValue(this),
            };
            field.RegisterValueChangedCallback(e => fieldInfo.SetValue(this, e.newValue));
            return field;
        }

        BaseField<string> CreateTextField(string label, FieldInfo fieldInfo)
        {
            var field = new TextField
            {
                label = label,
                value = fieldInfo.GetValue(this) as string,
            };
            field.RegisterValueChangedCallback(e => fieldInfo.SetValue(this, e.newValue));
            return field;
        }

        VisualElement CreateObjectField(string label, FieldInfo fieldInfo)
        {
            var field = new ObjectField
            {
                label = label,
                objectType = typeof(Object),
                value = fieldInfo.GetValue(this) as Object,
            };
            field.RegisterValueChangedCallback(e => fieldInfo.SetValue(this, e.newValue));
            return field;
        }
    }
}