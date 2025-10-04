using UnityEngine;

namespace Prototype
{
    [Data(typeof(RuntimeInstance_TransformToggleData))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Runtime Instance: Transform with Toggle")]
    public class RuntimeInstance_TransformToggle : RuntimeInstance_Transform
    {
        [SerializeField] bool value;

        public class RuntimeInstance_TransformToggleData : RuntimeInstance_TransformData
        {
            public bool value { get; set; } = true;
        }

        protected void OnLoad(RuntimeInstance_TransformToggleData data)
        {
            base.OnLoad(data);
            value = data.value;
        }

        protected void OnSave(RuntimeInstance_TransformToggleData data)
        {
            base.OnSave(data);
            data.value = value;
        }

        public void ToggleValue() => value = !value;
    }
}
