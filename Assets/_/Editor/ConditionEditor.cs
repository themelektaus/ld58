using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace Prototype.Editor
{
    using Button = UnityEngine.UIElements.Button;

    public abstract class ConditionEditor : UnityEditor.Editor
    {
        protected virtual bool showLogicGate => true;

        protected abstract VisualElement CreateInspectorGUI_If();

        SerializedProperty thenProperty;
        SerializedProperty elseProperty;

        VisualElement root;
        VisualElement thenElement;
        VisualElement elseElement;

        public override VisualElement CreateInspectorGUI()
        {
            thenProperty = serializedObject.FindProperty("then");
            elseProperty = serializedObject.FindProperty("else");

            root = new VisualElement();
            Refresh();
            return root;
        }

        static UnityEngine.Color GetActiveColor()
        {
            return new(0, .75f, 0, .4f);
        }

        void Refresh()
        {
            root.Clear();

            var element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;
            element.style.alignItems = Align.FlexStart;

            var @if = CreateInspectorGUI_If();
            @if.style.flexGrow = 1;
            element.Add(@if);

            element.Add(CreateInspectorGUI_LogicGate());
            root.Add(element);

            thenElement = CreateInspectorGUI_Then();
            thenElement.Show(thenProperty.arraySize > 0);
            root.Add(thenElement);

            elseElement = CreateInspectorGUI_Else();
            elseElement.Show(elseProperty.arraySize > 0);
            root.Add(elseElement);
        }

        VisualElement CreateInspectorGUI_LogicGate()
        {
            var property = serializedObject.FindProperty("logicGate");
            var value = property.enumValueIndex;

            var element = new VisualElement();
            element.style.flexDirection = FlexDirection.Column;

            if (showLogicGate)
            {
                var and = new Button() { text = "And" };
                if (value == 0)
                    and.style.backgroundColor = GetActiveColor();
                and.style.SetMargin(0);
                element.Add(and);

                var or = new Button() { text = "Or" };
                if (value == 1)
                    or.style.backgroundColor = GetActiveColor();
                or.style.SetMargin(0);
                element.Add(or);

                and.clicked += () =>
                {
                    property.enumValueIndex = 0;
                    serializedObject.ApplyModifiedProperties();

                    and.style.backgroundColor = GetActiveColor();
                    or.style.backgroundColor = StyleKeyword.Null;

                    and.Blur();
                };

                or.clicked += () =>
                {
                    property.enumValueIndex = 1;
                    serializedObject.ApplyModifiedProperties();

                    and.style.backgroundColor = StyleKeyword.Null;
                    or.style.backgroundColor = GetActiveColor();

                    or.Blur();
                };
            }

            if (thenProperty.arraySize == 0)
            {
                var thenButton = new Button { text = "+ Then" };
                thenButton.style.SetMargin(0);
                thenButton.style.backgroundColor = new UnityEngine.Color(.5f, .5f, 1, .25f);
                thenButton.clicked += () =>
                {
                    thenProperty.arraySize = 1;
                    serializedObject.ApplyModifiedProperties();
                    thenButton.RemoveFromHierarchy();
                    thenElement.Show(true);
                };
                element.Add(thenButton);
            }

            if (elseProperty.arraySize == 0)
            {
                var elseButton = new Button { text = "+ Else" };
                elseButton.style.SetMargin(0);
                elseButton.style.backgroundColor = new UnityEngine.Color(.5f, .5f, 1, .25f);
                elseButton.clicked += () =>
                {
                    elseProperty.arraySize = 1;
                    serializedObject.ApplyModifiedProperties();
                    elseButton.RemoveFromHierarchy();
                    elseElement.Show(true);
                };
                element.Add(elseButton);
            }

            return element;
        }

        VisualElement CreateInspectorGUI_Then()
        {
            var element = new VisualElement();

            var field = new PropertyField
            {
                bindingPath = "then"
            };
            field.BindProperty(serializedObject);

            element.Add(field);

            return element;
        }

        VisualElement CreateInspectorGUI_Else()
        {
            var element = new VisualElement();

            var field = new PropertyField
            {
                bindingPath = "else"
            };
            field.BindProperty(serializedObject);

            element.Add(field);

            return element;
        }
    }

    [CustomEditor(typeof(ReferenceCondition))]
    public class ReferenceConditionEditor : ConditionEditor
    {
        protected override VisualElement CreateInspectorGUI_If()
        {
            var element = new VisualElement();

            var field = new PropertyField
            {
                label = "If",
                bindingPath = "references"
            };
            field.BindProperty(serializedObject);

            element.Add(field);

            return element;
        }
    }

    [CustomEditor(typeof(HasLoadingSceneCondition))]
    public class HasLoadingSceneConditionEditor : ConditionEditor
    {
        protected override VisualElement CreateInspectorGUI_If()
        {
            return new();
        }
    }
}
