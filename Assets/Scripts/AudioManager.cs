using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField]private AudioMixer audioMixer; // Reference to Audio Mixer
    [SerializeField]private AudioSource audioSource;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep across scenes
            Instance = this;
            audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource dynamically
            audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    public void SetMusicVolume(float volume)
    {
        float dB = Mathf.Log10(volume) * 20;
        audioMixer.SetFloat("Music", dB);
        PlayerPrefs.SetFloat("musicVolume", volume);
        PlayerPrefs.Save();
    }
    public void PlaySong(AudioClip clip)
    {
        if (audioSource == null) return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }
}
