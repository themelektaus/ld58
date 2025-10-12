using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_GameState : MonoBehaviour
    {
        public enum Name { Scene, GameObject, Custom }
        public Name _name;

        public string customName;

        [SerializeField] bool applyOnAwake = true;

        void Awake()
        {
            if (applyOnAwake)
            {
                Apply();
            }
        }

        public void Apply()
        {
            var name = _name switch
            {
                Name.Scene => gameObject.scene.name,
                Name.GameObject => gameObject.name,
                _ => customName
            };

            if (name.EndsWith("(Clone)"))
            {
                name = name[..^7];
            }

            if (Time.frameCount == 0)
            {
                LD58_Global.GetSingletonInstance(x => x.gameStateMachine).SetState(name);
            }
            else
            {
                //error
                LD58_Global.GetSingletonInstance(x => x.gameStateMachine).Trigger(name);
            }
        }
    }
}
