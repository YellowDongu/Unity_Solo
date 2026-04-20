using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //===========================================
    // struct/enum
    //===========================================
    [System.Serializable]
    private struct Sound
    {
        public string name;
        public AudioClip clip;
    }

    //===========================================
    // Initializer/Destructor
    //===========================================

    private void Awake()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds/Effects");
        foreach (AudioClip sound in clips)
            sounds.TryAdd(sound.name, sound);

        clips = Resources.LoadAll<AudioClip>("Sounds/BGMs");
        foreach (AudioClip sound in clips)
            sounds.TryAdd(sound.name, sound);

        foreach (Sound sound in soundList)
            sounds.TryAdd(sound.name, sound.clip);

        soundList = null;
        ApplyVolume();
    }

    //===========================================
    // Methods
    //===========================================

    public void PlayOnce(string soundName)
    {
        if (!sounds.TryGetValue(soundName, out AudioClip clip))
            return;
        globalSFXSource.PlayOneShot(clip);
    }

    public void PlayOnce(AudioClip sound) { globalSFXSource.PlayOneShot(sound, worldSFXVolume); }

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


    public bool Play(AudioSource channel, string soundName, bool loop = true, bool forcePlay = true)
    {
        if (!sounds.TryGetValue(soundName, out AudioClip clip))
            return false;

        if (channel.isPlaying)
        {
            if (!forcePlay)
                return false;

            activeLoop = false;
            channel.Stop();
        }

        channel.clip = clip;
        channel.loop = loop;
        channel.Play();
        return true;
    }

    public bool Play(string soundName, bool loop = true, bool forcePlay = true) { return Play(globalAudioSource, soundName, loop, forcePlay); }
    public void Stop() { globalAudioSource.Stop(); }
    public void FadeOut(float speed) { StartCoroutine(SoundFadeOut(speed)); }
    private IEnumerator SoundFadeOut(float speed)
    {
        if (!globalAudioSource.isPlaying)
            yield break;

        float volume = globalAudioSource.volume;
        float percentage = 0.0f;

        while (percentage < 1.0f)
        {
            percentage = Mathf.Clamp01(percentage + speed * Time.deltaTime);
            globalAudioSource.volume = Mathf.Lerp(volume, 0.0f, percentage);
            yield return null;
        }

        globalAudioSource.Stop();
        globalAudioSource.volume = volume;
    }

    public void PlayLoop(string soundName, float loopBeginPercentage, float loopEndPercentage = 1.0f, bool forcePlay = true) { StartCoroutine(Loop(soundName, loopBeginPercentage, loopEndPercentage, forcePlay)); }
    private IEnumerator Loop(string soundName, float loopBeginPercentage, float loopEndPercentage, bool forcePlay)
    {
        if(!Play(globalAudioSource, soundName, false, forcePlay))
            yield break;
        
        while (inLoop)
            yield return null;

        float length = globalAudioSource.clip.length;

        inLoop = true;
        activeLoop = true;
        while (activeLoop)
        {
            float percentage = globalAudioSource.time / length;

            if (percentage >= loopEndPercentage)
                globalAudioSource.time = loopBeginPercentage * length;

            yield return null;
        }
        inLoop = false;
    }

    private void ApplyVolume()
    {
        globalAudioSource.volume = bgmVolume;
        globalSFXSource.volume = worldSFXVolume;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public AudioClip GetSound(string soundName) { if (sounds.TryGetValue(soundName, out AudioClip clip)) return clip; return null; }

    private bool activeLoop = false, inLoop = false;

    private Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    [SerializeField] private float bgmVolume = 0.5f;
    [SerializeField] private float worldSFXVolume = 1.0f;
    [SerializeField] private AudioSource globalAudioSource;
    [SerializeField] private AudioSource globalSFXSource;
    [SerializeField] private Sound[] soundList;
}
