using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Collections;

public class SpectralFluxBeatMap : MonoBehaviour
{
    public bool isSongFinished = false;
    public Action OnBeatmapGenerated;// Event to notify when the beatmap is generated
    public string songId;
    public GameObject notePrefab;         // Note prefab
    public Transform[] laneTransforms;    // Assign lane transforms in the Unity Inspector
    public AudioSource audioSource;
    public int fftSize = 1024;
    public float fallSpeed = 20f;
    public int fluxHistorySize = 20;
    private const int SPECTRUM_HISTORY_SIZE = 4;
    public float Threshold = 0.01f;
    public float bpm = 60f;
    private float secondsPerBeat;
    private float lastBeatTime;
    private bool isStopped = false; // To track if processing should stop

    [NonSerialized]
    private List<BeatData> detectedBeats = new List<BeatData>();

    [NonSerialized]
    private List<float> fluxHistory = new List<float>();

    [NonSerialized]
    private List<float[]> spectrumHistory = new List<float[]>();

    private static readonly float[] FREQUENCY_RANGES = { 20f, 40f, 60f, 100f, 300f, 16000f };

    [System.Serializable]
    public class BeatData
    {
        public float timestamp;
        public int lane;
        public float Flux;
        public float beatThreshold;
    }

    public class BeatList
    {
        public List<BeatData> beats;
        public int highScore;
        public float BPM;
        public string difficulty;
    }

    void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found in the scene.");
            return;
        }
        audioSource = AudioManager.Instance.GetAudioSource();
        isSongFinished = false;
        string selectedSongPath;
        if (SongManager.Instance.isPlaylistMode){
        selectedSongPath = SongManager.Instance.playlistSongPaths[SongManager.Instance.currentSongIndex];
        }else{
        selectedSongPath = SongManager.Instance.selectedSongPath;
        }
        if (!string.IsNullOrEmpty(selectedSongPath))
        {
            string fileName = Path.GetFileNameWithoutExtension(selectedSongPath) + ".json";
            string jsonFilePath = Path.Combine(Application.persistentDataPath, "Beatmaps", fileName);
            Debug.Log("JSON file path: " + jsonFilePath);
            if (File.Exists(jsonFilePath))
            {
                Debug.Log($"JSON file exists. Loading beatmap...");
                isStopped = true;
            }
            else
            {
                Debug.Log("JSON file does not exist. Starting real-time generation.");
                var audioClip = LoadAudioClip(selectedSongPath);
                InitializeAudioSource(audioClip);
            }

        }
    }

    void InitializeAudioSource(AudioClip clip)
    {
         if (audioSource == null) return;

        audioSource.clip = clip;
        bpm = 60f;
        secondsPerBeat = 60f / bpm;
        lastBeatTime = -secondsPerBeat;
        AudioManager.Instance.PlaySong(audioSource.clip);
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

    void Update()
    {
        if (isStopped) return;

        float[] currentSpectrum = new float[fftSize];
        audioSource.GetSpectrumData(currentSpectrum, 0, FFTWindow.Hamming); // ✅ Get real-time spectrum data

        UpdateSpectrumHistory(currentSpectrum);
        if (!HasEnoughSpectrumFrames()) return;

        float averageFlux = CalculateAverageFlux();
        fluxHistory.Add(averageFlux);
        if (fluxHistory.Count > fluxHistorySize) fluxHistory.RemoveAt(0);
        float mean = fluxHistory.Average();
        float std = Mathf.Sqrt(fluxHistory.Select(val => Mathf.Pow(val - mean, 2)).Average());
        float currentTime = audioSource.time;
        if (ShouldDetectBeat(currentSpectrum, currentTime, averageFlux, std))
        {
            RecordBeat(currentSpectrum, currentTime, averageFlux, std);
            CreateNoteInLane(detectedBeats[detectedBeats.Count - 1].lane);
            Debug.Log($"Target Time: {FormatTimestamp(currentTime)} Lane: {detectedBeats[detectedBeats.Count - 1].lane} flux: {averageFlux}");
        }
        if (audioSource.time >= audioSource.clip.length)
        {
            Debug.Log("End of audio reached. Detecting off-beats and stopping.");
            StartCoroutine(DelayBeforeSaveBeatsToJson(3f));
            isStopped = true; // Stop further execution
            if(SongManager.Instance.isPlaylistMode){
                StartCoroutine(GameManager.Instance.DelayBeforeNextSong(5f));
            }else{
            StartCoroutine(GameManager.Instance.DelayBeforeResultScreen(5f));
            }
        }
    }

    IEnumerator DelayBeforeSaveBeatsToJson(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for 'delay' seconds
        SaveBeatsToJson();
    }

    private void CreateNoteInLane(int lane)
    {
        Vector3 spawnPosition = new Vector3(
            laneTransforms[lane].position.x,
            laneTransforms[lane].position.y + 10f,
            laneTransforms[lane].position.z
        );

        GameObject note = Instantiate(notePrefab, spawnPosition, Quaternion.identity);
        note.GetComponent<Note>().fallSpeed = fallSpeed;
    }

    private string FormatTimestamp(float timestamp)
    {
        int minutes = Mathf.FloorToInt(timestamp / 60f);
        int seconds = Mathf.FloorToInt(timestamp % 60f);
        int milliseconds = Mathf.FloorToInt((timestamp % 1f) * 1000f);
        return $"{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
    }

    void UpdateSpectrumHistory(float[] currentSpectrum)
    {
        spectrumHistory.Add(currentSpectrum);
        if (spectrumHistory.Count > SPECTRUM_HISTORY_SIZE) spectrumHistory.RemoveAt(0);
    }

    bool HasEnoughSpectrumFrames()
    {
        return spectrumHistory.Count >= SPECTRUM_HISTORY_SIZE;
    }

    float CalculateAverageFlux()
    {
        float flux1 = CalculateSpectralFlux(spectrumHistory[3], spectrumHistory[2]);
        float flux2 = CalculateSpectralFlux(spectrumHistory[2], spectrumHistory[1]);
        float flux3 = CalculateSpectralFlux(spectrumHistory[1], spectrumHistory[0]);

        return (flux1 + flux2 + flux3) / 3f;
    }

    bool ShouldDetectBeat(float[] currentSpectrum, float timestamp, float averageFlux, float std) { return (averageFlux > Threshold + std && timestamp > 3f && timestamp - lastBeatTime > 0.3f); }
    void RecordBeat(float[] spectrum, float timestamp, float averageFlux, float std)
    {
        lastBeatTime = timestamp;
        detectedBeats.Add(new BeatData
        {
            timestamp = timestamp,
            lane = GetLaneFromFrequency(spectrum),
            Flux = averageFlux,
            beatThreshold = Threshold + std
        });
    }

    private float CalculateSpectralFlux(float[] currentFrame, float[] previousFrame)
    {
        float flux = 0f;
        // คำนวณ Nyquist Frequency
        float nyquistFrequency = audioSource.clip.frequency / 2f;
        // คำนวณ index สำหรับช่วงความถี่แต่ละเลน
        int startIndex1 = Mathf.Clamp(Mathf.CeilToInt(FREQUENCY_RANGES[0] / nyquistFrequency * (currentFrame.Length - 1)), 0, currentFrame.Length - 1);
        int endIndex1 = Mathf.Clamp(Mathf.FloorToInt(FREQUENCY_RANGES[1] / nyquistFrequency * (currentFrame.Length - 1)), 0, currentFrame.Length - 1);
        int startIndex2 = Mathf.Clamp(Mathf.CeilToInt(FREQUENCY_RANGES[1] / nyquistFrequency * (currentFrame.Length - 1)), 0, currentFrame.Length - 1);
        int endIndex2 = Mathf.Clamp(Mathf.FloorToInt(FREQUENCY_RANGES[2] / nyquistFrequency * (currentFrame.Length - 1)), 0, currentFrame.Length - 1);
        int startIndex3 = Mathf.Clamp(Mathf.CeilToInt(FREQUENCY_RANGES[2] / nyquistFrequency * (currentFrame.Length - 1)), 0, currentFrame.Length - 1);
        int endIndex3 = Mathf.Clamp(Mathf.FloorToInt(FREQUENCY_RANGES[3] / nyquistFrequency * (currentFrame.Length - 1)), 0, currentFrame.Length - 1);
        // คำนวณ flux สำหรับเลน 1 (ช่วงปกติ)
        for (int i = startIndex1; i <= endIndex2; i++)
        {
            float change = Mathf.Max(0, currentFrame[i] - previousFrame[i]);
            flux += change;
        }
        // คำนวณ flux สำหรับเลน 3 (ปรับลดเหลือ 5%)
        for (int i = startIndex3; i <= endIndex3; i++)
        {
            float change = Mathf.Max(0, currentFrame[i] - previousFrame[i]);
            flux += change * 0.05f;
        }
        return flux;
    }

    private int GetLaneFromFrequency(float[] spectrum)
    {
        int maxIndex = -1;
        float maxAmplitude = float.MinValue;
        float nyquistFrequency = audioSource.clip.frequency / 2f;

        for (int i = 0; i < laneTransforms.Length; i++)
        {
            int start = Mathf.Clamp(Mathf.CeilToInt(FREQUENCY_RANGES[i] / nyquistFrequency * (spectrum.Length - 1)), 0, spectrum.Length - 1);
            int end = Mathf.Clamp(Mathf.FloorToInt(FREQUENCY_RANGES[i + 1] / nyquistFrequency * (spectrum.Length - 1)), 0, spectrum.Length - 1);

            float peakAmplitude = spectrum.Skip(start).Take(end - start + 1).Max();
            if (peakAmplitude > maxAmplitude)
            {
                maxAmplitude = peakAmplitude;
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    void SaveBeatsToJson()
    {   
        string fileName;
        if(SongManager.Instance.isPlaylistMode){
            fileName = GenerateJsonFileName(SongManager.Instance.playlistSongTitles[SongManager.Instance.currentSongIndex]);
        }else{
            fileName = GenerateJsonFileName(SongManager.Instance.selectedSongTitle);
        }
        Debug.Log("Saving beats to " + fileName);

        string filePath = Path.Combine(Application.persistentDataPath, "Beatmaps");

        float newBPM = DetectOffBeats(detectedBeats);
        string newDifficulty;

        if(newBPM >= 140){
            newDifficulty = "Hard";
        } else if(newBPM >= 120){
            newDifficulty = "Medium";
        } else {
            newDifficulty = "Easy";
        }

        string json = JsonUtility.ToJson(new BeatList { beats = detectedBeats, highScore = SongManager.Instance.finalScore, BPM = newBPM, difficulty = newDifficulty }, true);
        File.WriteAllText(Application.persistentDataPath + "/Beatmaps/" + fileName, json);

        Debug.Log($"Beats saved to {filePath}");
    }

    string GenerateJsonFileName(string filePath)
    {
        string fileName = filePath;

        // Remove invalid characters for file names
        char[] invalidChars = Path.GetInvalidFileNameChars();
        fileName = string.Join("_", fileName.Split(invalidChars));

        // Add the .json extension
        return fileName + ".json";
    }
    float DetectOffBeats(List<BeatData> beats)
    {
        if (beats.Count == 0)
        {
            Debug.LogWarning("No beats detected!");
            return -1;
        }
        // ช่วง BPM ที่ต้องการค้นหา
        float minBPM = 40f;
        float maxBPM = 200f;
        float stepBPM = 1f;
        float bestBPM = 0f;
        float lowestOffBeatPercentage = float.MaxValue;
        // วนลูปหา BPM ที่มีเปอร์เซ็นต์ off-beats ต่ำที่สุด
        for (float bpmCandidate = minBPM; bpmCandidate <= maxBPM; bpmCandidate += stepBPM)
        {
            // สร้าง Beat Grid สำหรับ BPM ปัจจุบัน
            List<float> beatGrid = GenerateBeatGrid(bpmCandidate, audioSource.clip.length);
            // คำนวณเปอร์เซ็นต์ off-beats
            float offBeatCount = CalculateOffBeats(beats, beatGrid, 60f / bpmCandidate);
            float offBeatPercentage = offBeatCount * 100 / beats.Count;
            //Debug.Log($"Detection BPM: {bpmCandidate}. Off-Beat Percentage: {offBeatPercentage:F2}%");
            // อัปเดต BPM ที่ดีที่สุดถ้า off-beats ต่ำกว่า
            if (offBeatPercentage < lowestOffBeatPercentage)
            {
                lowestOffBeatPercentage = offBeatPercentage;
                bestBPM = bpmCandidate;
            }
        }
        Debug.Log($"Detection completed. Final BPM: {bestBPM}. Total Beats: {beats.Count}. Lowest Off-Beat Percentage: {lowestOffBeatPercentage:F2}%");
        return bestBPM;
    }
    // ฟังก์ชันสำหรับสร้าง Beat Grid
    List<float> GenerateBeatGrid(float bpm, float clipLength)
    {
        List<float> beatGrid = new List<float>();
        float secondsPerBeat = 60f / bpm;
        float currentBeat = 0;
        while (currentBeat <= clipLength)
        {
            beatGrid.Add(currentBeat);
            currentBeat += secondsPerBeat;
        }
        return beatGrid;
    }
    // ฟังก์ชันสำหรับคำนวณจำนวน off-beats
    float CalculateOffBeats(List<BeatData> beats, List<float> beatGrid, float secondsPerBeat)
    {
        float offBeatCount = 0f;
        foreach (var beat in beats)
        {
            // หา closest beat ใน beatGrid ที่ใกล้ที่สุด
            float closestBeat = beatGrid.OrderBy(b => Mathf.Abs(b - beat.timestamp)).First();
            // ถ้าห่างจาก closestBeat มากกว่า 25%
            if (Mathf.Abs(beat.timestamp - closestBeat) > secondsPerBeat * 0.25f)
            {
                offBeatCount += 1f;
            }
        }
        return offBeatCount;
    }
}
