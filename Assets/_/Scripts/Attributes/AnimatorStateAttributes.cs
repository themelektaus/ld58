using System;

namespace Prototype
{
    public abstract class AnimatorStateAttribute : Attribute
    {
        public readonly string[] states;
        public readonly int layerIndex;
        public readonly string layer;

        public AnimatorStateAttribute(string[] states)
        {
            this.states = states;
        }

        public AnimatorStateAttribute(int layerIndex, string state)
        {
            this.layerIndex = layerIndex;
            states = new[] { state };
        }

        public AnimatorStateAttribute(string layer, string state)
        {
            this.layer = layer;
            states = new[] { state };
        }
    }

    public class BeforeExitAttribute : AnimatorStateAttribute
    {
        public BeforeExitAttribute(string state) : base(new[] { state }) { }
        public BeforeExitAttribute(string[] states) : base(states) { }
        public BeforeExitAttribute(int layerIndex, string state) : base(layerIndex, state) { }
        public BeforeExitAttribute(string layer, string state) : base(layer, state) { }
    }

    public class AfterExitAttribute : AnimatorStateAttribute
    {
        public AfterExitAttribute(string state) : base(new[] { state }) { }
        public AfterExitAttribute(string[] states) : base(states) { }
        public AfterExitAttribute(int layerIndex, string state) : base(layerIndex, state) { }
        public AfterExitAttribute(string layer, string state) : base(layer, state) { }
    }

    public class BeforeEnterAttribute : AnimatorStateAttribute
    {
        public BeforeEnterAttribute(string state) : base(new[] { state }) { }
        public BeforeEnterAttribute(string[] states) : base(states) { }
        public BeforeEnterAttribute(int layerIndex, string state) : base(layerIndex, state) { }
        public BeforeEnterAttribute(string layer, string state) : base(layer, state) { }
    }

    public class AfterEnterAttribute : AnimatorStateAttribute
    {
        public AfterEnterAttribute(string state) : base(new[] { state }) { }
        public AfterEnterAttribute(string[] states) : base(states) { }
        public AfterEnterAttribute(int layerIndex, string state) : base(layerIndex, state) { }
        public AfterEnterAttribute(string layer, string state) : base(layer, state) { }
    }

    public class UpdateAttribute : AnimatorStateAttribute
    {
        public UpdateAttribute(string state) : base(new[] { state }) { }
        public UpdateAttribute(string[] states) : base(states) { }
        public UpdateAttribute(int layerIndex, string state) : base(layerIndex, state) { }
        public UpdateAttribute(string layer, string state) : base(layer, state) { }
    }

    public class FixedUpdateAttribute : AnimatorStateAttribute
    {
        public FixedUpdateAttribute(string state) : base(new[] { state }) { }
        public FixedUpdateAttribute(string[] states) : base(states) { }
        public FixedUpdateAttribute(int layerIndex, string state) : base(layerIndex, state) { }
        public FixedUpdateAttribute(string layer, string state) : base(layer, state) { }
    }

    public class LateUpdateAttribute : AnimatorStateAttribute
    {
        public LateUpdateAttribute(string state) : base(new[] { state }) { }
        public LateUpdateAttribute(string[] states) : base(states) { }
        public LateUpdateAttribute(int layerIndex, string state) : base(layerIndex, state) { }
        public LateUpdateAttribute(string layer, string state) : base(layer, state) { }
    }
}
