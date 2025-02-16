using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

public class NoteCreator : MonoBehaviour
{
    public bool isSongFinished = false;
    public GameObject notePrefab;         // Note prefab
    public Transform[] laneTransforms;    // Assign lane transforms in the Unity Inspector
    public float fallSpeed = 20f;         // Speed of the falling notes
    public AudioSource audioSource;      // The audio source for syncing

    [System.Serializable]
    public class BeatData
    {
        public float timestamp;    // Time of the detected beat in seconds
        public int lane;           // Lane corresponding to the frequency band
    }

    [System.Serializable]
    public class BeatList
    {
        public BeatData[] beats;   // Array of beat data
        public int highScore;
    }

    private int currentBeatIndex = 0; // Keeps track of which beat we're on
    private BeatList beatList;
    private float lastCheckedAudioTime = 0f; // Tracks the last audio time checked
    private bool isActive = false; // Flag to determine if the script should run
    

    void Start()
    {
        audioSource = AudioManager.Instance.GetAudioSource();
        isSongFinished = false;
        string songPath;
        if (SongManager.Instance.isPlaylistMode){
            songPath = SongManager.Instance.playlistSongPaths[SongManager.Instance.currentSongIndex];
        }else{
            songPath = SongManager.Instance.selectedSongPath;
        }
        AudioClip audioClip = LoadAudioClip(songPath); // Load the audio clip from the selected songPath;
        if (audioClip != null)
        {
        Debug.Log($"Loaded audio clip: {audioClip.name}");
        // Start beatmap loading process
        StartCoroutine(LoadBeatListAndStart(audioClip));
        }
    }

    IEnumerator LoadBeatListAndStart(AudioClip audioClip)
    {
        string fileName;
        if(SongManager.Instance.isPlaylistMode){
            fileName = SongManager.Instance.playlistSongTitles[SongManager.Instance.currentSongIndex] + ".json";
        }else{
            fileName = SongManager.Instance.selectedSongTitle + ".json";
        }
        string filePath = Path.Combine(Application.persistentDataPath, "Beatmaps", fileName);
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[Warning] JSON file not found at {filePath}. NoteCreator script will not run.");
            isActive = false; // Stop script execution
            yield break;
        }

        string json = File.ReadAllText(filePath);
        beatList = JsonUtility.FromJson<BeatList>(json);

        if (beatList == null || beatList.beats == null || beatList.beats.Length == 0)
        {
            Debug.LogWarning("[Warning] No beats found in the beat list! NoteCreator script will not run.");
            isActive = false; // Stop script execution
            yield break;
        }

        isActive = true; // Script can now run
        AudioManager.Instance.PlaySong(audioClip);
        Debug.Log($"[Start] Game starts, first beat at {beatList.beats[0].timestamp:F3}s");
    }

    void Update()
    {
        if (!isActive) return; // Stop the script if isActive is false

        if (currentBeatIndex >= beatList.beats.Length)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                Debug.Log($"Audio Time: {audioSource.time} / {audioSource.clip.length}");
                if(audioSource.time >= audioSource.clip.length - 0.1f)
                {
                isSongFinished = true;
                SaveHighscore(GameManager.Instance.GetScore());
                Debug.Log("Song is finished. Stopping NoteCreator script.");
                if(SongManager.Instance.isPlaylistMode){
                StartCoroutine(GameManager.Instance.DelayBeforeNextSong(2f));
                }else{
                StartCoroutine(GameManager.Instance.DelayBeforeResultScreen(5f));
                }
                }
            }
        }

        float currentAudioTime = audioSource.time; // Get the current playback time

        // Process beats that have passed since the last frame
        while (currentBeatIndex < beatList.beats.Length &&
               beatList.beats[currentBeatIndex].timestamp <= currentAudioTime)
        {
            BeatData currentBeat = beatList.beats[currentBeatIndex];

            if (currentBeat.lane >= 0 && currentBeat.lane < laneTransforms.Length)
            {
                CreateNoteInLane(currentBeat.lane, currentBeat.timestamp);
            }
            else
            {
                Debug.LogWarning($"[Warning] Invalid lane {currentBeat.lane} in beat data.");
            }

            currentBeatIndex++; // Move to the next beat
        }

        lastCheckedAudioTime = currentAudioTime; // Update the last checked time
    }

    void CreateNoteInLane(int lane, float timestamp)
    {
        Vector3 spawnPosition = new Vector3(
            laneTransforms[lane].position.x,
            laneTransforms[lane].position.y + 10f, // Spawns higher so it "falls"
            laneTransforms[lane].position.z
        );

        GameObject note = Instantiate(notePrefab, spawnPosition, Quaternion.identity);
        note.GetComponent<Note>().fallSpeed = fallSpeed;

        Debug.Log($"[Spawn] Note spawned at Lane: {lane}, Target Time: {FormatTimestamp(timestamp)}");
    }

    /// <summary>
    /// Formats a timestamp (seconds) to mm:ss:SSS
    /// </summary>
    private string FormatTimestamp(float timestamp)
    {
        int minutes = Mathf.FloorToInt(timestamp / 60f);
        int seconds = Mathf.FloorToInt(timestamp % 60f);
        int milliseconds = Mathf.FloorToInt((timestamp % 1f) * 1000f);
        return $"{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
    }
    private AudioClip LoadAudioClip(string filePath)
    {
        using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            www.SendWebRequest();
            while (!www.isDone) { }

            if (www.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerAudioClip.GetContent(www);
            }
            else
            {
                Debug.LogError($"Failed to load audio clip: {www.error}");
                return null;
            }
        }
    }

    void SaveHighscore(int newScore){
        string songTitle;
        if (SongManager.Instance.isPlaylistMode){
            songTitle = SongManager.Instance.playlistSongTitles[SongManager.Instance.currentSongIndex];
        }else{
            songTitle = SongManager.Instance.selectedSongTitle;
        }
        string jsonFilePath = Path.Combine(Application.persistentDataPath, "Beatmaps", songTitle + ".json");

        BeatList beatList;
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            beatList = JsonUtility.FromJson<BeatList>(json);

            if (newScore > beatList.highScore){
                beatList.highScore = newScore;
                string updatedJson = JsonUtility.ToJson(beatList, true);
                File.WriteAllText(jsonFilePath, updatedJson);
                Debug.Log($"High Score updated: {beatList.highScore} for {songTitle}");
            }
        }
    }
}
