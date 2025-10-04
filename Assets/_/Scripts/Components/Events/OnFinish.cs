using UnityEngine;
using UnityEngine.Playables;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Finish")]
    public class OnFinish : On
    {
        [SerializeField] Object @object;

        PlayableDirector playableDirector;

        void Awake()
        {
            if (@object is PlayableDirector playableDirector)
                this.playableDirector = playableDirector;
        }

        void OnEnable()
        {
            if (playableDirector)
                playableDirector.stopped += PlayableDirector_Stopped;
        }

        void OnDisable()
        {
            if (playableDirector)
                playableDirector.stopped -= PlayableDirector_Stopped;
        }

        void PlayableDirector_Stopped(PlayableDirector _)
        {
            Invoke();
        }
    }
}
