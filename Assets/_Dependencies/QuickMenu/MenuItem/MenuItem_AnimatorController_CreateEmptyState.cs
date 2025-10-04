using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace QuickMenu
{
    internal class MenuItem_AnimatorController_CreateEmptyState : MenuItem
    {
        public override string title => "Empty State";

        public override string category => "Create";

        public string name = "New Empty State";

        public override bool Validation(Context context)
        {
            return context.@object is AnimatorStateMachine;
        }

        public override bool Command(Context context)
        {
            var stateMachine = context.@object as AnimatorStateMachine;
            stateMachine.AddState(name);
            return true;
        }
    }
}