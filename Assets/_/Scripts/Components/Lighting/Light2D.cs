using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype
{
    public abstract class Light2D : MonoBehaviour
    {
        const int LIGHT_CAPACITY = 20;

        protected const int TYPE_NONE = 0;
        protected const int TYPE_GLOBAL = 1;
        protected const int TYPE_POINT = 2;
        protected const int TYPE_SPOT = 3;

        static Light2D master;

        static List<Object> _listeners;
        static List<Object> listeners => _listeners ??= new();
        static List<Light2D> _lights;
        static List<Light2D> lights => _lights ??= new();
        static List<Light2D> activeLights;

        static readonly Vector4[] lights_Position = new Vector4[LIGHT_CAPACITY];
        static readonly Vector4[] lights_Color = new Vector4[LIGHT_CAPACITY];
        static readonly float[] lights_Type = new float[LIGHT_CAPACITY];
        static readonly Vector4[] lights_Properties1 = new Vector4[LIGHT_CAPACITY];
        static readonly Vector4[] lights_Properties2 = new Vector4[LIGHT_CAPACITY];
        static readonly Vector4[] lights_Properties3 = new Vector4[LIGHT_CAPACITY];

        public static bool hasGlobalLight => lights.Any(x => x.type == TYPE_GLOBAL);

        [SerializeField, ReadOnlyInPlayMode]
        Object listener;

        public Color color = Color.white;

        protected abstract int type { get; }
        protected abstract float[] properties { get; }

        void OnDisable()
        {
            if (master == this)
                master = null;

            if (listener)
                listeners.Remove(listener);

            lights.Remove(this);

            UpdateShader();
            UpdateMaterials();
        }

        protected virtual void Update()
        {
            if (!master)
                master = this;

            if (listener && !listeners.Contains(listener))
                listeners.Add(listener);

            if (!lights.Contains(this))
            {
                lights.Add(this);
                UpdateShader();
                UpdateMaterials();
                return;
            }

            if (master != this)
                return;

            master.UpdateShader();
            master.UpdateMaterials();
        }

        void UpdateShader()
        {
            (activeLights ??= new()).Clear();

            var listenerTransform = listeners
                .Where(x => x)
                .Select(x => x.GetTransform())
                .LastOrDefault(x => x);

            if (listenerTransform)
            {
                activeLights.AddRange(
                    lights
                        .Where(x => x.type != TYPE_NONE)
                        .OrderByDescending(x => x.type == TYPE_GLOBAL)
                        .ThenBy(x => (x.transform.position - listenerTransform.position).sqrMagnitude)
                );
            }
            else
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    var sceneView = SceneView.currentDrawingSceneView;
                    if (!sceneView)
                        sceneView = SceneView.lastActiveSceneView;

                    if (sceneView && sceneView.camera)
                    {
                        activeLights.AddRange(
                            lights
                                .Where(x => x.type != TYPE_NONE)
                                .OrderByDescending(x => x.type == TYPE_GLOBAL)
                                .ThenBy(x => (x.transform.position - sceneView.camera.transform.position).sqrMagnitude)
                        );
                    }
                }
#endif
                if (activeLights.Count == 0)
                    activeLights.AddRange(lights.OrderBy(x => x.type));
            }

#if UNITY_EDITOR
            var activeScene = SceneManager.GetActiveScene();
            activeLights.RemoveAll(x => x.gameObject.scene != activeScene);
#endif

            for (var i = 0; i < LIGHT_CAPACITY; i++)
            {
                void SetDefaultValues()
                {
                    lights_Position[i] = default;
                    lights_Color[i] = default;
                    lights_Type[i] = default;
                    lights_Properties1[i] = default;
                    lights_Properties2[i] = default;
                    lights_Properties3[i] = default;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (!IsSceneLightingEnabled())
                    {
                        SetDefaultValues();
                        continue;
                    }
                }
#endif
                if (i >= activeLights.Count)
                {
                    SetDefaultValues();
                    continue;
                }

                lights_Position[i] = activeLights[i].transform.position;
                lights_Color[i] = activeLights[i].color;
                lights_Type[i] = activeLights[i].type;

                var p = activeLights[i].properties;

                lights_Properties1[i].x = p.Length > 0 ? p[0] : 0;
                lights_Properties1[i].y = p.Length > 1 ? p[1] : 0;
                lights_Properties1[i].z = p.Length > 2 ? p[2] : 0;
                lights_Properties1[i].w = p.Length > 3 ? p[3] : 0;

                lights_Properties2[i].x = p.Length > 4 ? p[4] : 0;
                lights_Properties2[i].y = p.Length > 5 ? p[5] : 0;
                lights_Properties2[i].z = p.Length > 6 ? p[6] : 0;
                lights_Properties2[i].w = p.Length > 7 ? p[7] : 0;

                lights_Properties3[i].x = p.Length > 8 ? p[8] : 0;
                lights_Properties3[i].y = p.Length > 9 ? p[9] : 0;
                lights_Properties3[i].z = p.Length > 10 ? p[10] : 0;
                lights_Properties3[i].w = p.Length > 11 ? p[11] : 0;
            }
        }

        void UpdateMaterials()
        {
            Shader.SetGlobalVectorArray("_Prototype_2D_Lights_Position", lights_Position);
            Shader.SetGlobalVectorArray("_Prototype_2D_Lights_Color", lights_Color);
            Shader.SetGlobalFloatArray("_Prototype_2D_Lights_Type", lights_Type);
            Shader.SetGlobalVectorArray("_Prototype_2D_Lights_Properties1", lights_Properties1);
            Shader.SetGlobalVectorArray("_Prototype_2D_Lights_Properties2", lights_Properties2);
            Shader.SetGlobalVectorArray("_Prototype_2D_Lights_Properties3", lights_Properties3);
        }

#if UNITY_EDITOR
        static bool IsSceneLightingEnabled()
        {
            var sceneView = SceneView.currentDrawingSceneView;
            if (!sceneView)
                sceneView = SceneView.lastActiveSceneView;

            if (!sceneView)
                return false;

            if (!sceneView.sceneLighting)
                return false;

            return true;
        }
#endif
    }
}
