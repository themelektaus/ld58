using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Top-Down Character Controller (2D): Footsteps")]
    public class TopDownCharacterFootsteps2D : MonoBehaviour
    {
        [SerializeField] SoundEffect soundEffect;
        [SerializeField] float interval = .1f;

        TopDownCharacterController2D controller;
        float lastTime;

        void Awake()
        {
            controller = GetComponentInParent<TopDownCharacterController2D>();
        }

        void Update()
        {
            if (!controller.isMoving)
            {
                lastTime = 0;
                return;
            }

            if (lastTime > Time.time - interval)
                return;

            lastTime = Time.time;
            soundEffect.PlayClipAt(controller.transform);
        }
    }
}
