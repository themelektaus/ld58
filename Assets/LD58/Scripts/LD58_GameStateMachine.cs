using UnityEngine;
using UnityEngine.Audio;

namespace Prototype.LD58
{
    public class LD58_GameStateMachine : AnimatorStateBehaviour
    {
        [SerializeField] AudioMixer audioMixer;

        readonly SmoothFloat musicPass = new(1, .2f);

        protected override void OnUpdate()
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            musicPass.target = HasState("Settings") ? 1 : 0;
            musicPass.Update();

            audioMixer.SetFloat("Music Lowpass", Mathf.Lerp(22000, 800, musicPass));
            audioMixer.SetFloat("Music Highpass", Mathf.Lerp(10, 300, musicPass));
#endif
        }
    }
}
