using Prototype;
using Prototype.LD58;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class LD58_Music : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;

    public enum OnAwakeAction { DoNothing, PlayFirst, PlayRandom }
    [SerializeField] OnAwakeAction onAwake;

    public enum OnFinishTrackAction { DoNothing, PlayNext, PlayRandom }
    [SerializeField] OnFinishTrackAction onFinishTrack;

    [SerializeField] AudioMixerGroup audioMixerGroup;
    [SerializeField] float fadeSpeed = .2f;
    [SerializeField] List<Track> tracks;

    [SerializeField, ReadOnly] List<TrackInstance> deadTrackInstances = new();
    [SerializeField, ReadOnly] List<Track> shuffledTracks = new();

    TrackInstance activeTrackInstance;

    [Serializable]
    public class Track
    {
        public AudioClip clip;
        [Range(0, 1)] public float volume = .8f;
    }

    [Serializable]
    public class TrackInstance
    {
        public Track track;
        public AudioSource audioSource;
    }

    [ContextMenu("Play Next")]
    public void PlayNext()
    {
        PlayNextInternal(random: false);
    }

    [ContextMenu("Play Next Random")]
    public void PlayNextRandom()
    {
        PlayNextInternal(random: true);

    }

    void PlayNextInternal(bool random)
    {
        var tracks = random ? shuffledTracks : this.tracks;

        if (tracks.Count == 0)
        {
            return;
        }

        var index = activeTrackInstance is null
            ? 0
            : tracks.IndexOf(activeTrackInstance.track) + 1;

        if (index >= tracks.Count)
        {
            index -= tracks.Count;
        }

        Play(tracks[index]);
    }

    public void Play(string name)
    {
        Play(tracks.FirstOrDefault(x => x.clip.name == name));
    }

    public void Play(Track track)
    {
        if (track is null)
        {
            return;
        }

        if (activeTrackInstance is not null && activeTrackInstance.track == track)
        {
            return;
        }

        Stop();

        var x = gameObject.AddComponent<AudioSource>();
        x.playOnAwake = false;
        x.loop = true;
        x.volume = 0;
        x.spatialBlend = 0;
        x.outputAudioMixerGroup = audioMixerGroup;
        x.clip = track.clip;
        x.Play();

        activeTrackInstance = new()
        {
            track = track,
            audioSource = x
        };
    }

    [ContextMenu("Stop")]
    public void Stop()
    {
        if (activeTrackInstance is not null)
        {
            deadTrackInstances.Add(activeTrackInstance);
            activeTrackInstance = null;
        }
    }

    void Awake()
    {
        activeTrackInstance = null;
        deadTrackInstances.Clear();

        shuffledTracks = tracks.ToList();
        shuffledTracks.Shuffle();

        if (onAwake == OnAwakeAction.PlayFirst)
        {
            PlayNext();
        }
        else if (onAwake == OnAwakeAction.PlayRandom)
        {
            PlayNextRandom();
        }
    }

    void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            var key = "Sound Volume";

            if (audioMixer.GetFloat(key, out var volume))
            {
                audioMixer.SetFloat(key, volume / 4);
            }
        }
    }

    void Update()
    {
        if (activeTrackInstance is not null)
        {
            var audioSource = activeTrackInstance.audioSource;
            var volume = activeTrackInstance.track.volume;
            audioSource.volume = Mathf.Min(audioSource.volume + Time.deltaTime * fadeSpeed, volume);

            if (audioSource.time >= audioSource.clip.length - (1 / fadeSpeed))
            {
                if (onFinishTrack == OnFinishTrackAction.PlayNext)
                {
                    PlayNext();
                }
                else if (onFinishTrack == OnFinishTrackAction.PlayRandom)
                {
                    PlayNextRandom();
                }
            }
        }

        foreach (var x in deadTrackInstances)
        {
            if (x.audioSource.volume > 0)
            {
                x.audioSource.volume -= Time.deltaTime * fadeSpeed;
            }
            else
            {
                x.audioSource.Destroy();
                x.audioSource = null;
            }
        }

        deadTrackInstances.RemoveAll(x => !x.audioSource);
    }
}
