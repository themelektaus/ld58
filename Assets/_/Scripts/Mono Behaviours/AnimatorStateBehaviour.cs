using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

using Flags = System.Reflection.BindingFlags;

namespace Prototype
{
    public abstract class AnimatorStateBehaviour : MonoBehaviour
    {
        public struct Weight
        {
            public float layer;
            public float state;

            public Weight(Animator animator, int layerIndex)
            {
                layer = animator.GetLayerWeight(layerIndex);
                state = animator.IsInTransition(layerIndex) ? animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime : 1;
            }
        }

        static readonly System.Type[] attributeTypes = typeof(AnimatorStateAttribute)
            .Assembly
            .GetTypes()
            .Where(x => x.IsSubclassOf(typeof(AnimatorStateAttribute)))
            .ToArray();

        [SerializeField, ReadOnlyInPlayMode] protected Animator animator;

        readonly Dictionary<int, AnimatorStateInfo> oldState = new();
        readonly Dictionary<int, AnimatorStateInfo> oldNextState = new();

        readonly Dictionary<System.Type, Dictionary<(int layerIndex, string state), List<MethodInfo>>> methods = new();

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate() { }

        protected void Awake()
        {
            if (!animator)
                animator = GetComponent<Animator>();

            animator.keepAnimatorStateOnDisable = true;

            for (int i = 0; i < animator.layerCount; i++)
            {
                oldState[i] = animator.GetCurrentAnimatorStateInfo(i);
                oldNextState[i] = animator.GetNextAnimatorStateInfo(i);
            }

            var methods = GetType().GetMethods(Flags.Public | Flags.NonPublic | Flags.Instance);
            foreach (var method in methods)
            {
                foreach (var attributeType in attributeTypes)
                {
                    var attribute = method.GetCustomAttribute(attributeType, true) as AnimatorStateAttribute;
                    if (attribute is null)
                        continue;

                    if (!this.methods.ContainsKey(attributeType))
                        this.methods.Add(attributeType, new());

                    var layerIndex = attribute.layerIndex;
                    if (attribute.layer is not null)
                        layerIndex = animator.GetLayerIndex(attribute.layer);

                    foreach (var state in attribute.states)
                    {
                        var key = (layerIndex, state);
                        var methodDictionary = this.methods[attributeType];
                        if (!methodDictionary.ContainsKey(key))
                            methodDictionary.Add(key, new());
                        methodDictionary[key].Add(method);
                    }
                }
            }

            OnAwake();

            for (int i = 0; i < animator.layerCount; i++)
            {
                var state = animator.GetCurrentAnimatorStateInfo(i);
                Invoke<BeforeEnterAttribute>(i, state);
            }
        }

        protected void Start()
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                var state = animator.GetCurrentAnimatorStateInfo(i);
                Invoke<AfterEnterAttribute>(i, state);
            }

            OnStart();
        }

        void Invoke<T>(
            int layerIndex,
            AnimatorStateInfo state,
            params System.Type[] requiredParameters
        ) where T : AnimatorStateAttribute
        {
            Invoke<T>(layerIndex, state, new(), requiredParameters);
        }

        void Invoke<T>(
            int layerIndex,
            AnimatorStateInfo state,
            AnimatorStateInfo referenceState,
            params System.Type[] requiredParameters
        ) where T : AnimatorStateAttribute
        {
            if (this.methods.Count == 0)
                return;

            var type = typeof(T);
            if (!this.methods.ContainsKey(type))
                return;

            var methods = this.methods[type];
            if (methods.Count == 0)
                return;

            foreach (var key in methods.Keys)
            {
                if (key.layerIndex != layerIndex)
                    continue;

                var hasState = state.IsName(key.state);

                foreach (var method in methods[key])
                {
                    //void DebugLog() => Debug.Log($"[{typeof(T).Name[..^9]}] {key.state}");

                    var parameters = method.GetParameters();

                    if (requiredParameters.Length > 0)
                    {
                        if (!parameters.Select(x => x.ParameterType).SequenceEqual(requiredParameters))
                            continue;
                    }

                    if (parameters.Length == 0)
                    {
                        if (hasState)
                        {
                            //DebugLog();
                            method.Invoke(this, null);
                        }
                        continue;
                    }

                    var forceInvoke = false;
                    var parameterValues = new List<object>();

                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType == typeof(bool))
                        {
                            forceInvoke = true;
                            parameterValues.Add(hasState);
                            continue;
                        }

                        if (parameter.ParameterType == typeof(AnimatorStateInfo))
                        {
                            parameterValues.Add(referenceState);
                            continue;
                        }

                        if (parameter.ParameterType == typeof(Weight))
                        {
                            parameterValues.Add(new Weight(animator, layerIndex));
                            continue;
                        }
                    }

                    if (forceInvoke || hasState)
                    {
                        //DebugLog();
                        method.Invoke(this, parameterValues.ToArray());
                    }
                }
            }
        }

        protected void Update()
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                var isInTransition = animator.IsInTransition(i);

                UpdateTransitions(i, isInTransition);

                if (isInTransition)
                    Invoke<UpdateAttribute>(i, oldState[i], typeof(Weight));
                else
                    Invoke<UpdateAttribute>(i, oldState[i]);
            }

            OnUpdate();
        }

        protected void FixedUpdate()
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.IsInTransition(i))
                    Invoke<FixedUpdateAttribute>(i, oldState[i], typeof(Weight));
                else
                    Invoke<FixedUpdateAttribute>(i, oldState[i]);
            }

            OnFixedUpdate();
        }

        protected void LateUpdate()
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.IsInTransition(i))
                    Invoke<LateUpdateAttribute>(i, oldState[i], typeof(Weight));
                else
                    Invoke<LateUpdateAttribute>(i, oldState[i]);
            }

            OnLateUpdate();
        }

        public void SetState(string name)
        {
            animator.Play(name, 0);
        }

        public void Trigger(string name)
        {
            if (animator.gameObject.activeSelf)
                animator.SetTrigger(name);
        }

        public void Toggle(string name)
        {
            animator.SetBool(name, !animator.GetBool(name));
        }

        public void Enable(string name)
        {
            animator.SetBool(name, true);
        }

        public void Disable(string name)
        {
            animator.SetBool(name, false);
        }

        void UpdateTransitions(int i, bool isInTransition)
        {
            var oldState = this.oldState[i];
            var newState = animator.GetCurrentAnimatorStateInfo(i);

            var oldNextState = this.oldNextState[i];
            var newNextState = animator.GetNextAnimatorStateInfo(i);

            if (isInTransition)
            {
                var doAfter = oldNextState.fullPathHash != 0 && newNextState.fullPathHash == 0;
                var doBefore = oldNextState.fullPathHash != newNextState.fullPathHash;

                if (doBefore)
                    Invoke<BeforeExitAttribute>(i, oldState, referenceState: newNextState);

                if (doAfter)
                    Invoke<AfterExitAttribute>(i, oldNextState, referenceState: newState);

                if (doBefore)
                {
                    Invoke<BeforeEnterAttribute>(i, newNextState, referenceState: oldState);
                    this.oldNextState[i] = newNextState;
                }

                if (doAfter)
                {
                    Invoke<AfterEnterAttribute>(i, newState, referenceState: oldNextState);
                    this.oldNextState[i] = newNextState;
                }

                return;
            }

            if (oldState.fullPathHash != newState.fullPathHash)
            {
                var beforeAttribute = oldNextState.fullPathHash == 0;
                var nextAttribute = newNextState.fullPathHash == 0;

                if (beforeAttribute)
                    Invoke<BeforeExitAttribute>(i, oldState, referenceState: newState);

                if (nextAttribute)
                    Invoke<AfterExitAttribute>(i, oldState, referenceState: newState);

                if (beforeAttribute)
                    Invoke<BeforeEnterAttribute>(i, newState, referenceState: oldState);

                if (nextAttribute)
                    Invoke<AfterEnterAttribute>(i, newState, referenceState: oldState);

                this.oldState[i] = newState;
                this.oldNextState[i] = newNextState;
            }
        }

        public bool HasState(string name)
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetCurrentAnimatorStateInfo(i).IsName(name))
                    return true;
            }
            return false;
        }

        protected void GotoScene(string from, string to, int loadingSceneIndex)
        {
            this.While(() => LoadingScene.current)
                .Start(() => SceneSwitcher.GotoScene(from, to, loadingSceneIndex));
        }
    }
}
