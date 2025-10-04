using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prototype.LD58
{
    [AddComponentMenu("LD58: Game State Machine")]
    public class LD58_GameStateMachine : AnimatorStateBehaviour
    {
        LevelUpScreen levelUpScreen;

        [AfterEnter("Ingame")]
        void AfterEnter_Ingame(AnimatorStateInfo referenceState)
        {
            if (referenceState.IsName("Settings"))
            {
                return;
            }

            levelUpScreen = FindAnyObjectByType<LevelUpScreen>(FindObjectsInactive.Include);
        }

        int level;

        [Update("Ingame")]
        void Update_Ingame()
        {
            if (levelUpScreen.IsActiveOrEnabled())
            {
                return;
            }

            var level = LD58_Global.instance.data.GetLevel();

            if (this.level < level)
            {
                this.level++;
                levelUpScreen.gameObject.SetActive(true);
            }
        }
    }
}
