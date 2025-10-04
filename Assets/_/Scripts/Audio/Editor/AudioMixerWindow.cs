using System.Collections.Generic;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace Prototype.Editor
{
    public class AudioMixerWindow : EditorWindow
    {
        [MenuItem("Tools/Audio Mixer")]
        static void Init()
        {
            GetWindow<AudioMixerWindow>().titleContent = new("Audio Mixer");
        }

        AudioMixerGroup audioMixerGroup;

        public enum SoundType { SoundEffect, SoundEffectCollection }
        SoundType soundType;

        List<SoundEffect> soundEffects;
        List<SoundEffectCollection> soundEffectCollections;

        readonly List<(SoundEffect soundEffect, VisualElement element)> soundEffectTracks = new();
        readonly List<(SoundEffectCollection soundEffect, VisualElement element)> soundEffectCollectionTracks = new();

        void OnEnable()
        {
            audioMixerGroup = null;
            soundType = SoundType.SoundEffect;

            soundEffects = null;
            soundEffectCollections = null;

            RefreshLayout();
        }

        void RefreshLayout()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath(
                    AssetDatabase.FindAssets(
                        $"t:{nameof(VisualTreeAsset)} {nameof(AudioMixerWindow)}"
                    )[0]
                )
            ).Instantiate();

            rootVisualElement.Clear();
            rootVisualElement.Add(root);

            root.StretchToParentSize();

            root.Q<ObjectField>().RegisterValueChangedCallback(x =>
            {
                audioMixerGroup = x.newValue as AudioMixerGroup;
                RefreshTracksVisibility();
            });

            root.Q<EnumField>().RegisterValueChangedCallback(x =>
            {
                soundType = (SoundType) x.newValue;
                RefreshTracksVisibility();
            });
        }

        void OnInspectorUpdate()
        {
            var refreshTracksVisibility = false;

            if (soundEffects is null)
            {
                soundEffects = new();

                foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(SoundEffect)}"))
                {
                    soundEffects.Add(
                        AssetDatabase.LoadAssetAtPath<SoundEffect>(
                            AssetDatabase.GUIDToAssetPath(guid)
                        )
                    );
                }

                RefreshSoundEffectTracks();
                refreshTracksVisibility = true;
            }

            if (soundEffectCollections is null)
            {
                soundEffectCollections = new();

                foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(SoundEffectCollection)}"))
                {
                    soundEffectCollections.Add(
                        AssetDatabase.LoadAssetAtPath<SoundEffectCollection>(
                            AssetDatabase.GUIDToAssetPath(guid)
                        )
                    );
                }

                RefreshSoundEffectCollectionTracks();
                refreshTracksVisibility = true;
            }

            if (refreshTracksVisibility)
                RefreshTracksVisibility();
        }

        void RefreshSoundEffectTracks()
        {
            soundEffectTracks.Clear();

            if (soundEffects is not null)
            {
                foreach (var soundEffect in soundEffects)
                {
                    var track = new VisualElement();
                    track.AddToClassList("track");
                    var applyButton = new Button(soundEffect.ApplyVolumeMultiplierFactor)
                    {
                        text = "Apply"
                    };
                    applyButton.AddToClassList("accent");
                    track.Add(applyButton);
                    track.Add(
                        new AudioMixerSliderGUI
                        {
                            @object = soundEffect,
                            propertyPath = "volumeMultiplierFactor",
                            valueRange = new Vector2(0, 2),
                            defaultValue = 1
                        }
                    );
                    track.Add(
                        new Label(
                            soundEffect.name
                                .Replace(" (Sound Effect)", "")
                                .Replace(" - BGM", "")
                                .Replace(" - SFX", "")
                        )
                    );
                    track.Add(
                        new Button(() => Selection.activeObject = soundEffect)
                        {
                            text = "Select"
                        }
                    );
                    soundEffectTracks.Add((soundEffect, track));
                }
            }
        }

        void RefreshSoundEffectCollectionTracks()
        {
            soundEffectCollectionTracks.Clear();

            if (soundEffectCollections is not null)
            {
                foreach (var soundEffectCollection in soundEffectCollections)
                {
                    var track = new VisualElement();
                    track.AddToClassList("track");
                    var disabledButton = new Button
                    {
                        text = "-",
                        focusable = false,
                        pickingMode = PickingMode.Ignore,
                    };
                    disabledButton.AddToClassList("disabled");
                    track.Add(disabledButton);
                    track.Add(
                        new AudioMixerSliderGUI
                        {
                            @object = soundEffectCollection,
                            propertyPath = "additionalGain",
                            valueRange = new Vector2(-1, 1)
                        }
                    );
                    track.Add(
                        new Label(
                            soundEffectCollection.name
                                .Replace(" (Sound Effect Collection)", "")
                                .Replace(" Sound Effect", "")
                                .Replace(" - BGM Collection", "")
                                .Replace(" - SFX Collection", "")
                        )
                    );
                    track.Add(
                        new Button(() => Selection.activeObject = soundEffectCollection)
                        {
                            text = "Select"
                        }
                    );
                    soundEffectCollectionTracks.Add((soundEffectCollection, track));
                }
            }
        }

        void RefreshTracksVisibility()
        {
            var content = rootVisualElement.Q<ScrollView>().contentContainer;
            content.Clear();

            switch (soundType)
            {
                case SoundType.SoundEffect:
                    foreach (var (soundEffect, track) in soundEffectTracks)
                    {
                        if (audioMixerGroup)
                        {
                            var audioMixerGroup = soundEffect.audioMixerGroup;
                            if (!audioMixerGroup && soundEffect.collection)
                                audioMixerGroup = soundEffect.collection.defaultAudioMixerGroup;
                            if (this.audioMixerGroup != audioMixerGroup)
                                continue;
                        }
                        content.Add(track);
                    }
                    break;

                case SoundType.SoundEffectCollection:
                    foreach (var (soundEffectCollection, track) in soundEffectCollectionTracks)
                    {
                        if (audioMixerGroup)
                        {
                            if (audioMixerGroup != soundEffectCollection.defaultAudioMixerGroup)
                                continue;
                        }
                        content.Add(track);
                    }
                    break;
            }
        }
    }
}
