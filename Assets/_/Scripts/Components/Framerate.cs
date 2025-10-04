using System;

#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
#endif

using UnityEngine;

namespace Prototype
{
    [ExecuteAlways]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Framerate")]
    public class Framerate : MonoBehaviour
    {
        public int targetFrameRate = 60;
        public bool vSync = true;
        public bool fullscreen = true;

        int hash;

#if UNITY_EDITOR
        Type[] fullscreenEditorTypes;
#endif

        void Awake()
        {
#if !ENABLE_IL2CPP
            if (!PlayerPrefs.HasKey(nameof(vSync)))
                PlayerPrefs.SetInt(nameof(vSync), vSync ? 1 : 0);

            if (!PlayerPrefs.HasKey(nameof(fullscreen)))
                PlayerPrefs.SetInt(nameof(fullscreen), fullscreen ? 1 : 0);
#endif
            ResetHash();

            if (!enabled)
                OnDisable();
        }

#if UNITY_EDITOR
        void OnDestroy()
        {
            if (fullscreen && Application.isPlaying)
            {
                var containers = fullscreenEditorTypes?
                    .FirstOrDefault(x => x.Name == "Fullscreen")?
                    .GetMethod("GetAllFullscreen")?
                    .Invoke(null, new object[] { true, true }) as object[];

                if (containers is not null)
                {
                    foreach (var container in containers)
                    {
                        container.GetType()
                            .GetMethod("Close")?
                            .Invoke(container, Array.Empty<object>());
                    }
                }
            }
        }
#endif

        void ResetHash()
        {
            hash = HashCode.Combine(-1, -1);
        }

        void OnEnable()
        {
            ResetHash();
#if !ENABLE_IL2CPP
            vSync = PlayerPrefs.GetInt(nameof(vSync)) != 0;
            fullscreen = PlayerPrefs.GetInt(nameof(fullscreen)) != 0;
#endif
            Update();
        }

        void OnDisable()
        {
#if !ENABLE_IL2CPP
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
#endif
            ResetHash();
        }

        void Update()
        {
            var hash = HashCode.Combine(targetFrameRate, vSync, fullscreen);
            if (this.hash == hash)
                return;

            this.hash = hash;

#if !ENABLE_IL2CPP
            PlayerPrefs.SetInt(nameof(vSync), vSync ? 1 : 0);
            PlayerPrefs.SetInt(nameof(fullscreen), fullscreen ? 1 : 0);
            PlayerPrefs.Save();
#endif

            Application.targetFrameRate = vSync ? -1 : targetFrameRate;
            QualitySettings.vSyncCount = vSync ? 1 : 0;
            UpdateFullscreen();
        }

        public void UpdateFullscreen()
        {
#if UNITY_EDITOR
            if (fullscreen && Application.isPlaying)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.Load("FullscreenEditor");
                }
                catch
                {
                    assembly = null;
                }
                fullscreenEditorTypes = assembly?.GetTypes();
                fullscreenEditorTypes?
                    .FirstOrDefault(x => x.Name == "MenuItems")?
                    .GetMethod("GVMenuItem", BindingFlags.Static | BindingFlags.NonPublic)?
                    .Invoke(null, Array.Empty<object>());
            }
#elif !ENABLE_IL2CPP
            Screen.fullScreenMode = fullscreen
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;
#endif
        }

    }
}
