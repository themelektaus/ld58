using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.Editor
{
    [CustomEditor(typeof(AnimatorStateChangeEvent))]
    public class AnimatorStateChangeEventEditor : UnityEditor.Editor
    {
        AnimatorStateChangeEvent component => target as AnimatorStateChangeEvent;

        public override VisualElement CreateInspectorGUI()
        {
            var stateProperty = serializedObject.FindProperty("state");
            if (stateProperty.stringValue.IsNullOrEmpty())
            {
                stateProperty.stringValue = "-";
                serializedObject.ApplyModifiedProperties();
            }

            var root = this.CreateRootElement(new(0, .5f, 1, .05f));

            root.AddVisualTreeAsset<AnimatorStateChangeEventEditor>();

            var controller = GetAnimatorController();

            var layerIndexField = root.Q<DropdownField>("LayerIndex");
            if (controller)
                layerIndexField.choices = GetLayerIndexChoices(controller);

            var stateField = root.Q<DropdownField>("State");
            if (controller)
                stateField.choices = GetStateChoices(controller);

            root.Bind(serializedObject);

            if (controller)
                layerIndexField.RegisterValueChangedCallback(x => stateField.choices = GetStateChoices(controller));

            return root;
        }

        AnimatorController GetAnimatorController()
        {
            var animator = component.GetComponent<Animator>();
            if (!animator)
                return null;

            var runtimeController = animator.runtimeAnimatorController;
            if (!runtimeController)
                return null;

            return runtimeController as AnimatorController;
        }

        List<string> GetLayerIndexChoices(AnimatorController controller)
        {
            var result = new List<string> { "0" };
            for (int i = 1; i < controller.layers.Length; i++)
                result.Add(i.ToString());
            return result;
        }

        List<string> GetStateChoices(AnimatorController controller)
        {
            var result = new List<string> { "-" };

            if (!int.TryParse(serializedObject.FindProperty("layerIndex").stringValue, out var layerIndex))
                return result;

            if (layerIndex < 0 || controller.layers.Length <= layerIndex)
                return result;

            var layer = controller.layers[layerIndex];
            var states = layer.stateMachine.states;
            result.AddRange(states.Select(x => x.state.name));

            return result;
        }
    }
}
