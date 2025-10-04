using UnityEngine;

namespace Prototype
{
    [CreateAssetMenu(menuName = "Prototype/Sound Effect Distance Range")]
    public class SoundEffectDistanceRange : UnityEngine.ScriptableObject
    {
        public Vector2 range = new SoundEffectOptions().GetDistanceRange();
    }
}
