using Prototype;
using Prototype.LD58;
using UnityEngine;

public class Trashbin : MonoBehaviour
{
    public TrashObject.Type type;

    public RoboAvatar roboAvatar;

    [SerializeField] SoundEffect sound1;
    [SerializeField] SoundEffect sound2;

    void OnTriggerEnter2D(Collider2D collision)
    {
        var trashObject = collision.GetComponentInParent<TrashObject>();
        if (!trashObject)
        {
            return;
        }

        var simpleAnimation = GetComponentInChildren<SimpleAnimation>();
        simpleAnimation.Stop();
        simpleAnimation.Play();

        if (trashObject.currentGrabber)
        {
            trashObject.currentGrabber.DropTrashObject(trashObject);
        }

        if (trashObject.type == type)
        {
            sound1.PlayRandomClipAt(transform.position);

            LD58_Global.instance.data.Collect(type);
            roboAvatar.happiness = 1;
            roboAvatar.sadness = 0;
        }
        else
        {
            sound2.PlayRandomClipAt(transform.position);

            LD58_Global.instance.data.Malus(type);
            roboAvatar.happiness = 0;
            roboAvatar.sadness = 1;
        }

        trashObject.gameObject.Destroy();
    }
}
