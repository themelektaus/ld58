using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    public static class OverrideBehaviourReset
    {
        public static bool active;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize() => active = true;
    }

    // T => target object, S => value
    public abstract class OverrideBehaviour<T, S> : MonoBehaviour
    {
        static readonly Dictionary<T, List<OverrideBehaviour<T, S>>> slaves = new();

        [SerializeField, ReadOnly] bool isMaster;
        [SerializeField, ReadOnly] S originalValue;

        [Range(0, 1)] public float weight;

        protected virtual void OnAwake() { }

        protected virtual bool useLateUpdate => false;
        protected virtual bool useMaxWeight => false;

        protected abstract T GetTarget();
        protected abstract void Setup(out S originalValue);
        protected abstract void Process(ref S value, float weight);
        protected abstract void Apply(in S value);

        protected void Awake()
        {
            isMaster = false;

            if (OverrideBehaviourReset.active)
            {
                OverrideBehaviourReset.active = false;
                slaves.Clear();
            }

            Setup(out originalValue);

            OnAwake();
        }

        void OnEnable()
        {
            var target = GetTarget();

            if (!slaves.ContainsKey(target))
                slaves.Add(target, new());

            slaves[target].Add(this);

            if (slaves[target].Any(x => x.isMaster))
                return;

            isMaster = true;
        }

        void OnDisable()
        {
            var target = GetTarget();
            slaves[target].Remove(this);

            if (!isMaster)
                return;

            isMaster = false;

            var slave = slaves[target].FirstOrDefault();
            if (slave)
                slave.isMaster = true;
        }

        void Update()
        {
            if (!useLateUpdate)
                UpdateInternal();
        }

        void LateUpdate()
        {
            if (useLateUpdate)
                UpdateInternal();
        }

        void UpdateInternal()
        {
            this.weight = Mathf.Clamp01(this.weight);

            if (!isMaster)
                return;

            var value = originalValue;

            var target = GetTarget();
            float weight = -1;

            if (useMaxWeight)
            {
                OverrideBehaviour<T, S> processingSlave = null;
                foreach (var slave in slaves[target])
                {
                    if (weight > slave.weight)
                        continue;

                    weight = slave.weight;
                    processingSlave = slave;
                }

                if (processingSlave)
                    processingSlave.Process(ref value, processingSlave.weight);
            }
            else
            {
                weight = 0;

                foreach (var slave in slaves[target])
                {
                    slave.Process(ref value, slave.weight / (1 + weight));
                    weight += slave.weight;
                }
            }

            Apply(in value);
        }

        public void SetWeight(float weight) => this.weight = weight;
    }
}
