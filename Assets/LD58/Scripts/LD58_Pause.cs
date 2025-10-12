using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_Pause : MonoBehaviour
    {
        [SerializeField] LD58_Player player;

        void OnEnable()
        {
            player.Pause();
        }

        void OnDisable()
        {
            player.Resume();
        }
    }
}
