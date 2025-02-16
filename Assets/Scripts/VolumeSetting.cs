using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
        }
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        SetVolume(volume);
    }
    public void SetVolume(float volume) => AudioManager.Instance.SetMusicVolume(volume);

    public void LoadVolume(){
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");

        SetVolume(musicSlider.value);
    }
}
