using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_Recycler : MonoBehaviour
    {
        public LD58_Player player;
        public LD58_TrashInfo trashInfo;

        public LD58_RoboHead roboAvatar;

        [SerializeField] SoundEffect sound1;
        [SerializeField] SoundEffect sound2;

        [SerializeField] SimpleAnimation simpleAnimation;

        void OnValidate()
        {
            if (!player)
            {
                player = FindAnyObjectByType<LD58_Player>();
            }

            if (!roboAvatar)
            {
                roboAvatar = FindAnyObjectByType<LD58_RoboHead>();
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            var trashObject = collision.GetComponentInParent<LD58_Trash>();
            if (!trashObject)
            {
                return;
            }

            simpleAnimation.Stop();
            simpleAnimation.Play();

            if (trashObject.currentGrabber)
            {
                trashObject.currentGrabber.DropTrashObject(trashObject);
            }

            if (trashObject.trashInfo == trashInfo)
            {
                sound1.PlayRandomClipAt(transform.position);

                player.Collect(trashInfo);
                roboAvatar.happiness = 1;
            }
            else
            {
                sound2.PlayRandomClipAt(transform.position);

                roboAvatar.happiness = 0;
                roboAvatar.sadness = 1;
            }

            trashObject.gameObject.Destroy();
        }
    }
}
