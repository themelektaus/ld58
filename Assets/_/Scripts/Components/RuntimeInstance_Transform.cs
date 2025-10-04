using UnityEngine;

namespace Prototype
{
    [Data(typeof(RuntimeInstance_TransformData))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Runtime Instance: Transform")]
    public class RuntimeInstance_Transform : RuntimeInstance
    {
        public class RuntimeInstance_TransformData : Data
        {
            public float[] position { get; set; } = new float[3];
        }

        protected void OnLoad(RuntimeInstance_TransformData data)
        {
            var p = data.position;
            transform.position = new(p[0], p[1], p[2]);
        }

        protected void OnSave(RuntimeInstance_TransformData data)
        {
            var p = transform.position;
            data.position = new[] { p.x, p.y, p.z };
        }
    }
}
