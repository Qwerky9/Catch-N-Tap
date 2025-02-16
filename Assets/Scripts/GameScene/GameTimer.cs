using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public AudioSource audioSource ;
    public TextMeshProUGUI timeText;
    public bool timeIsRunning = false;
    private float timeRemaining;

    void Start()
    {
        audioSource = AudioManager.Instance.GetAudioSource();
        if (audioSource != null)
        {
            timeRemaining = audioSource.clip.length; // Set timer to song length
            timeIsRunning = true;
        }
    }

    void Update()
    {
        if (timeIsRunning && timeRemaining > 0.01f)
        {
            timeRemaining -= Time.unscaledDeltaTime;
            DisplayTime(timeRemaining);
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
