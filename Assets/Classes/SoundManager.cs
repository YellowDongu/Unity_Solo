using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    private struct Sound
    {
        public string name;
        public AudioClip clip;
    }

    private void Awake()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds/Effects");
        foreach (AudioClip sound in clips)
        {
            sounds.TryAdd(sound.name, sound);
        }

        foreach (Sound sound in soundList)
        {
            sounds.TryAdd(sound.name, sound.clip);
        }
        soundList = null;
    }

    public void PlayOnce(string soundName)
    {
        if (!sounds.TryGetValue(soundName, out AudioClip clip))
            return;
        globalAudioSource.PlayOneShot(clip);
    }

    public void PlayOnce(AudioSource channel, AudioClip sound)
    {
        if (channel == null || sound == null)
            return;
        channel.PlayOneShot(sound);
    }

    public void PlayOnce(string soundName, Vector3 position)
    {
        if (!sounds.TryGetValue(soundName, out AudioClip clip))
            return;

        AudioSource.PlayClipAtPoint(clip, position, worldSFXVolume);
    }

    public void PlayOnce(AudioSource channel, string soundName)
    {
        if (channel == null || !sounds.TryGetValue(soundName, out AudioClip clip))
            return;
        channel.PlayOneShot(clip);
    }

    public void Play(string soundName, bool loop = true, bool forcePlay = true)
    {
        if(globalAudioSource.isPlaying || !forcePlay) return;

        if (!sounds.TryGetValue(soundName, out AudioClip clip))
            return;

        if (forcePlay)
            globalAudioSource.Stop();

        globalAudioSource.Play();
        globalAudioSource.PlayOneShot(clip);
        globalAudioSource.loop = loop;
    }


    public AudioClip GetSound(string soundName) { if (sounds.TryGetValue(soundName, out AudioClip clip)) return clip; return null; }


    private Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    [SerializeField] private float volume;
    [SerializeField] private float worldSFXVolume = 0.5f;
    [SerializeField] private Sound[] soundList;
    [SerializeField] private AudioSource globalAudioSource;
}
