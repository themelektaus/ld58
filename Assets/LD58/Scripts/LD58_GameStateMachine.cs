using UnityEngine;
using UnityEngine.Audio;

namespace Prototype.LD58
{
    public class LD58_GameStateMachine : AnimatorStateBehaviour, IObserver<LD58_Player.PauseChangeMessage>
    {
        [SerializeField] AudioMixer audioMixer;

        readonly SmoothFloat musicPass = new(1, .2f);
        bool playerIsPaused;

        public void ReceiveNotification(LD58_Player.PauseChangeMessage message)
        {
            playerIsPaused = message.isPaused;
        }

        void OnEnable()
        {
            LD58_Player.onPauseChange.Register(this);
        }

        void OnDisable()
        {
            LD58_Player.onPauseChange.Unregister(this);
        }

        protected override void OnUpdate()
        {
            musicPass.target = ((HasState("Ingame") && !playerIsPaused) || HasState("Title")) ? 0 : 1;
            musicPass.Update();

            audioMixer.SetFloat("Music Lowpass", Mathf.Lerp(22000, 800, musicPass));
            audioMixer.SetFloat("Music Highpass", Mathf.Lerp(10, 300, musicPass));
        }
    }
}
