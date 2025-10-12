using System.Collections.Generic;
using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_PlayerPerkDisplayContainer : MonoBehaviour, IObserver<LD58_PlayerPerkInfo.LevelUpMessage>
    {
        [SerializeField] LD58_Player player;
        [SerializeField] LD58_PlayerPerkDisplay playerPerkDisplay;
        [SerializeField] int spacing = 100;

        readonly Dictionary<LD58_PlayerPerkInfo, LD58_PlayerPerkDisplay> instances = new();

        void OnEnable()
        {
            LD58_PlayerPerkInfo.onLevelUp.Register(this);
        }

        void OnDisable()
        {
            LD58_PlayerPerkInfo.onLevelUp.Unregister(this);
        }

        public void ReceiveNotification(LD58_PlayerPerkInfo.LevelUpMessage message)
        {
            if (instances.ContainsKey(message.info))
            {
                if (message.info.currentLevel == 0)
                {
                    instances[message.info].gameObject.Kill();
                    instances.Remove(message.info);
                }
            }
            else
            {
                if (message.info.currentLevel > 0)
                {
                    var display = playerPerkDisplay.gameObject
                        .Instantiate(parent: transform)
                        .GetComponent<LD58_PlayerPerkDisplay>();

                    display.info = message.info;

                    var position = display.transform.localPosition;
                    position.y -= spacing * instances.Count;
                    display.transform.localPosition = position;

                    instances.Add(message.info, display);
                }
            }
        }
    }
}
