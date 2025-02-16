using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SongCardManager : MonoBehaviour
{
    public string songTitle; // Title of the song
    public string songFilePath;// Path to the song file
    public string id; // Unique identifier
    public int highScore;
    public string difficulty;
    public Sprite thumbnailImage;
    public GameObject highlight;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI difficultyText;
    public Image thumbnail;

    private SongSelectionManager selectionManager;
    private bool isSelected = false;

    public void Start(){
        // Update UI elements
        titleText.text = songTitle;
        highScoreText.text = "High Score: " + highScore;
        difficultyText.text = "Difficulty: " + difficulty;
        thumbnail.sprite = thumbnailImage;

        selectionManager = FindObjectOfType<SongSelectionManager>();
        if (highlight != null)
            highlight.SetActive(false); // Hide highlight initially
    }

    public void OnClick(){
        if (selectionManager == null) return;

        if (SongManager.Instance.isPlaylistMode){
            ToggleSelectionForPlaylist();
        }
        else{
            OnCardSelected();
        }
    }

    public void OnCardSelected(){
        // Debug.Log("Card selected: " + songTitle);
        // Debug.Log("Song Path: " + songFilePath);
        selectionManager.SelectCard(this);
    }

    public void SetHighlight(bool isActive){
        highlight.SetActive(isActive);
    }

    public void ToggleSelectionForPlaylist()
    {
        if (isSelected)
        {
            selectionManager.RemoveFromPlaylist(this);
            isSelected = false;
        }
        else
        {
            isSelected = selectionManager.AddToPlaylist(this);
        }
    }
}

