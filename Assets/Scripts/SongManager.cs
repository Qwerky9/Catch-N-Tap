
using System.Collections.Generic;
using UnityEngine;
public class SongManager : MonoBehaviour
{
    public static SongManager Instance;
    public string selectedSongPath;
    public string selectedSongTitle;
    public List<string> playlistSongPaths = new List<string>();
    public List<string> playlistSongTitles = new List<string>();
    public List<int> playlistScores = new List<int>();
    public List<int> playlistMaxCombos = new List<int>();
    public List<bool> playlistPass = new List<bool>();
    public int currentSongIndex = 0;
    public int finalScore;
    public int maxCombo;
    public bool isPlaylistMode = false;
    public bool pass = true;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    public void SetSelectedSong(string path, string title){
        isPlaylistMode = false;
        selectedSongPath = path.Replace("\\", "/");
        selectedSongTitle = title;
        // Reset playlist data in case switching back from playlist mode
        currentSongIndex = 0;
    }

    public void SetPlaylist(List<string> paths, List<string> titles)
    {
        isPlaylistMode = true;

          // Limit the playlist to a max of 4 songs
        int maxSongs = Mathf.Min(paths.Count, 4);
        playlistSongPaths = paths.GetRange(0, maxSongs);
        playlistSongTitles = titles.GetRange(0, maxSongs);

        currentSongIndex = 0;
        Debug.Log($"[SongManager] Playlist Mode Selected: {playlistSongTitles.Count} songs");
    }

    public void MoveToNextSong()
    {
        playlistScores[currentSongIndex] = finalScore;
        playlistMaxCombos[currentSongIndex] = maxCombo;
        playlistPass[currentSongIndex] = pass;
        currentSongIndex++;
        
        if (currentSongIndex < playlistSongPaths.Count)
        {
            // Load the next song
            selectedSongPath = playlistSongPaths[currentSongIndex];
            selectedSongTitle = playlistSongTitles[currentSongIndex];

            Debug.Log($"[SongManager] Now Playing: {selectedSongTitle}");
        
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); // Reload gameplay
        }
        else
        {
            // All songs have been played, load the result screen
            UnityEngine.SceneManagement.SceneManager.LoadScene("PlaylistResultScreen");
        }
    }

    public void ResetInstance()
    {
        selectedSongPath = null;
        selectedSongTitle = null;
        playlistSongPaths.Clear();
        playlistSongTitles.Clear();
        playlistScores.Clear();
        playlistMaxCombos.Clear();
        currentSongIndex = 0;
        finalScore = 0;
        maxCombo = 0;
        pass = true;
    }
}
