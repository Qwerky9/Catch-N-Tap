using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

  public class BeatList
    {
        public int highScore;
        public string difficulty;
    }
public class SongSelectionManager : MonoBehaviour
{
    public bool isPlaylistMode = false;
    public int maxPlaytlistSongs = 4;
    public GameObject songCardPrefab;
    public Transform contentPanel;
    public Sprite defaultThumbnail;
    public Button playButton;
    public GameObject goToFolderButton;
    public GameObject noSongsMessage;
    private SongCardManager selectedCard;
    private List<string> loadedsongs = new List<string>();
    private string songFolderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Songs");
    
    void Start()
    {   
        playButton.interactable = false;
        SongManager.Instance.ResetInstance();
        LoadSongsFromFolder();
    }
    void LoadSongsFromFolder(){
        loadedsongs.Clear();

        if (!Directory.Exists(songFolderPath)){
            Directory.CreateDirectory(songFolderPath);
            Debug.LogWarning("Song folder not found at" + songFolderPath);
            return;
        }else{
            Debug.Log("Song folder found at" + songFolderPath);
        }

        string[] files = Directory.GetFiles(songFolderPath, "*.mp3");

        if(files.Length == 0){
            Debug.LogWarning("No songs found in the song folder.");
            ShowNoSongsUI();
        }
        else
        {
            foreach (string file in files){
            if(loadedsongs.Contains(file)) 
                continue;

            // add song to the list
            loadedsongs.Add(file);

            // create song card
            CreateSongCard(file);
            }
        }
       
    }

    void ShowNoSongsUI(){
        noSongsMessage.SetActive(true); // Show a UI element that tells the user there are no songs in the folder
        goToFolderButton.SetActive(true);
    }

    void CreateSongCard(string filePath){
        GameObject newCard = Instantiate(songCardPrefab, contentPanel);
        
        SongCardManager cardManager = newCard.GetComponent<SongCardManager>();

        if (cardManager != null){
            // Set song data
            cardManager.songTitle = Path.GetFileNameWithoutExtension(filePath);
            cardManager.songFilePath = filePath;

            string jsonFilePath = Path.Combine(Application.persistentDataPath + "/Beatmaps", cardManager.songTitle + ".json");
            if (File.Exists(jsonFilePath)){
                string json = File.ReadAllText(jsonFilePath);
                BeatList beatList = JsonUtility.FromJson<BeatList>(json);
                cardManager.highScore = beatList.highScore;
                cardManager.difficulty = beatList.difficulty;
            }
            else{
            cardManager.highScore = 0; // Default value, could be loaded from save data
            cardManager.difficulty = "Unknown"; // Default value, can be updated dynamically
            } 

            // Optional: Set thumbnail (use a default image or load custom metadata)
            cardManager.thumbnailImage = defaultThumbnail;

            Debug.Log($"Loaded song: {cardManager.songTitle}");
        }
    }

    public void SelectCard(SongCardManager cardManager){
        if (selectedCard != null){
            selectedCard.SetHighlight(false);
        }
        selectedCard = cardManager;
        selectedCard.SetHighlight(true);
        playButton.interactable = true;
    }

    public bool AddToPlaylist(SongCardManager cardManager){
        if (SongManager.Instance.playlistSongPaths.Count >= 4){
            return false;
        }

        SongManager.Instance.playlistSongPaths.Add(cardManager.songFilePath);
        SongManager.Instance.playlistSongTitles.Add(cardManager.songTitle);
        SongManager.Instance.playlistScores.Add(0);
        SongManager.Instance.playlistMaxCombos.Add(0);
        SongManager.Instance.playlistPass.Add(false);
        selectedCard = cardManager;
        selectedCard.SetHighlight(true);
        playButton.interactable = true;
        return true;
    }

    public void RemoveFromPlaylist(SongCardManager cardManager){
            if (selectedCard != null){
                SongManager.Instance.playlistSongPaths.Remove(cardManager.songFilePath);
                SongManager.Instance.playlistSongTitles.Remove(cardManager.songTitle);
                SongManager.Instance.playlistMaxCombos.Remove(0);
                SongManager.Instance.playlistScores.Remove(0);
                SongManager.Instance.playlistPass.Remove(false);
                selectedCard = cardManager;
                selectedCard.SetHighlight(false);
                if(SongManager.Instance.playlistSongPaths.Count <= 0){
                    playButton.interactable = false;
                }
            }

    }

    public void PlaySelectedSong(){
        if (selectedCard != null){
            if(SongManager.Instance.isPlaylistMode){
            Debug.Log("Playing song: " + selectedCard.songTitle);
            Debug.Log("Song Path: " + selectedCard.songFilePath);
            SongManager.Instance.SetPlaylist(SongManager.Instance.playlistSongPaths, SongManager.Instance.playlistSongTitles);
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }else{
            Debug.Log("Playing song: " + selectedCard.songTitle);
            Debug.Log("Song Path: " + selectedCard.songFilePath);
            SongManager.Instance.SetSelectedSong(selectedCard.songFilePath, selectedCard.songTitle);
            // Load the gameplay scene or start playing the song
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }
        }
    }

    public void GoBack(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OpenSongsFolder()
    {
        Application.OpenURL(songFolderPath); // Open folder in File Explorer
    }
}
