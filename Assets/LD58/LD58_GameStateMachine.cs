using UnityEngine;

namespace Prototype.LD58
{
    [AddComponentMenu("LD58: Game State Machine")]
    public class LD58_GameStateMachine : AnimatorStateBehaviour
    {
        [AfterEnter("Ingame")]
        public void AfterEnter_Ingame(AnimatorStateInfo referenceState)
        {
            if (!referenceState.IsName("Settings"))
            {
                level = 0;
                LD58_Global.instance.ResetGame();
            }

            LD58_Global.instance.Resume();
        }

        [BeforeExit("Ingame")]
        void BeforeExit_Ingame()
        {
            LD58_Global.instance.Pause();
        }

        int level;

        [Update("Ingame")]
        void Update_Ingame()
        {
            if (LD58_Global.instance.IsPaused())
            {

                return;
            }

            var level = LD58_Global.instance.data.GetLevel();

            if (this.level >= level)
            {
                return;
            }

            this.level++;
            LD58_Global.instance.Pause();

            this.Wait(.2f).Start(() =>
            {
                FindAnyObjectByType<LevelUpScreen>(FindObjectsInactive.Include).gameObject.SetActive(true);
            });
        }
    }
}
