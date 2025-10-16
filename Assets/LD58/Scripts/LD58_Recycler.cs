using System.Collections.Generic;
using System.Linq;
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

        readonly Queue<LD58_Trash> trashQueue = new();
        readonly Timer trashCollectTimer = new();

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!player)
            {
                player = this.EnumerateSceneObjectsByType<LD58_Player>().FirstOrDefault();
                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (!roboAvatar)
            {
                roboAvatar = this.EnumerateSceneObjectsByType<LD58_RoboHead>().FirstOrDefault();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif

        void OnTriggerEnter2D(Collider2D collision)
        {
            var trash = collision.GetComponentInParent<LD58_Trash>();
            if (trash && !trashQueue.Contains(trash))
            {
                trashQueue.Enqueue(trash);
            }
        }

        void Update()
        {
            if (trashCollectTimer.Update(.2f))
            {
                if (trashQueue.Count > 0)
                {
                    Collect(trashQueue.Dequeue());
                }
            }
        }

        void Collect(LD58_Trash trash)
        {
            simpleAnimation.Stop();
            simpleAnimation.Play();

            if (trash.currentGrabber)
            {
                trash.currentGrabber.DropTrashObject(trash);
            }

            if (trash.trashInfo == trashInfo)
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

                player.points.neverSad = false;
            }

            trash.gameObject.Destroy();
        }
    }
}
