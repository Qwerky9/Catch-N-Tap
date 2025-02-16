using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeatmapLoader : MonoBehaviour
{
    public AudioSource sharedAudioSource; // The single AudioSource used for playback
    public SpectralFluxBeatMap spectralFluxScript; // Reference to SpectralFluxBeatMap
    public NoteCreator noteCreatorScript; // Reference to NoteCreator

    void Start()
    {
        // Assign the shared AudioSource to both scripts
        spectralFluxScript.audioSource = sharedAudioSource;
        noteCreatorScript.audioSource = sharedAudioSource;

        // Handle which system to activate
        if (File.Exists(GetBeatmapPath()))
        {
            // Disable SpectralFluxBeatMap and enable NoteCreator
            spectralFluxScript.gameObject.SetActive(false);
            noteCreatorScript.gameObject.SetActive(true);
        }
        else
        {
            // Enable SpectralFluxBeatMap and disable NoteCreator
            spectralFluxScript.gameObject.SetActive(true);
            noteCreatorScript.gameObject.SetActive(false);
        }
    }

    string GetBeatmapPath()
    {
        string fileName = Path.GetFileNameWithoutExtension(SongManager.Instance.selectedSongPath) + ".json";
        return Path.Combine(Application.persistentDataPath, "Beatmaps", fileName);
    }
}
