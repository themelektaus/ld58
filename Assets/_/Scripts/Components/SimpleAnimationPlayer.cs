using System.Linq;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Simple Animation Player")]
    public class SimpleAnimationPlayer : MonoBehaviour
    {
        [SerializeField] SimpleAnimation[] animations;
        [SerializeField] bool playOnEnable = true;
        [SerializeField] bool playReverseOnDisable = true;

        public float avgTime => animations.Average(x => x.time);

        void OnEnable()
        {
            if (playOnEnable)
                PlayAll();
        }

        void OnDisable()
        {
            if (playReverseOnDisable)
                foreach (var animation in animations)
                    animation.PlayReverse();
        }

        public bool IsPlaying()
        {
            foreach (var animation in animations)
                if (animation.isPlaying)
                    return true;
            return false;
        }

        public void PlayAll()
        {
            foreach (var animation in animations)
                animation.Play();
        }

        public void StopAll()
        {
            foreach (var animation in animations)
                animation.Stop();
        }
    }
}
