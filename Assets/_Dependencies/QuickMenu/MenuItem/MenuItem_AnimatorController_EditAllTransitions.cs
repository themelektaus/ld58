using System.Collections.Generic;
using UnityEditor.Animations;

namespace QuickMenu
{
    internal class MenuItem_AnimatorController_EditAllTransitions : MenuItem
    {
        public override string title => "Edit All Transitions";

        public bool hasExitTime = false;
        public float exitTime = 1;
        public bool hasFixedDuration = true;
        public float duration = 0;
        public float offset = 0;

        public override bool Validation(Context context)
        {
            if (!context.isAnimatorControllerTool)
                return false;

            return true;
        }

        public override bool Command(Context context)
        {
            var allStates = new List<AnimatorState>();
            var allTransitions = new List<AnimatorStateTransition>();
            var newTransitions = new List<AnimatorStateTransition>();

            foreach (var layer in (context.asset as AnimatorController).layers)
            {
                foreach (var subStateMachine in layer.stateMachine.stateMachines)
                {
                    foreach (var state in subStateMachine.stateMachine.states)
                        foreach (var transition in state.state.transitions)
                            PrepareTransition(allTransitions, newTransitions, transition);

                    foreach (var transition in subStateMachine.stateMachine.anyStateTransitions)
                        PrepareTransition(allTransitions, newTransitions, transition);
                }

                foreach (var transition in layer.stateMachine.anyStateTransitions)
                    PrepareTransition(allTransitions, newTransitions, transition);

                foreach (var subStateMachine in layer.stateMachine.stateMachines)
                    foreach (var state in subStateMachine.stateMachine.states)
                        IterateAnimator(allStates, allTransitions, newTransitions, state);
                
                foreach (var state in layer.stateMachine.states)
                    IterateAnimator(allStates, allTransitions, newTransitions, state);
            }

            foreach (var transition in newTransitions)
            {
                transition.hasExitTime = hasExitTime;
                transition.exitTime = exitTime;
                transition.hasFixedDuration = true;
                transition.duration = duration;
                transition.offset = offset;
            }

            return true;
        }

        static void PrepareTransition(
            List<AnimatorStateTransition> allTransitions,
            List<AnimatorStateTransition> newTransitions,
            AnimatorStateTransition transition
        )
        {
            allTransitions.Add(transition);
            if (string.IsNullOrWhiteSpace(transition.name))
                newTransitions.Add(transition);
        }

        static void IterateAnimator(
            List<AnimatorState> allStates,
            List<AnimatorStateTransition> allTransitions,
            List<AnimatorStateTransition> newTransitions,
            ChildAnimatorState state
        )
        {
            allStates.Add(state.state);
            foreach (var transition in state.state.transitions)
            {
                allTransitions.Add(transition);
                if (string.IsNullOrWhiteSpace(transition.name))
                    newTransitions.Add(transition);
            }
        }
    }
}