using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FolderInitializer : MonoBehaviour
{
    
    private void Start()
    {
        CreateSongsFolder("Songs");  // Creates "Songs" folder
        CreateBeatmapFolder("Beatmaps"); // Creates "Beatmaps" folder
        SceneManager.LoadSceneAsync("TitleScreen");
    }

    private void CreateBeatmapFolder(string folderName)
    {
        // Get the root directory of the game (where the .exe is)
        string rootPath = Application.persistentDataPath;

        // Define the folder path
        string folderPath = Path.Combine(rootPath, folderName);

        // Check if the folder exists, if not, create it
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"Created '{folderName}' folder at: {folderPath}");
        }
        else
        {
            Debug.Log($"'{folderName}' folder already exists at: {folderPath}");
        }
    }

    private void CreateSongsFolder(string folderName)
    {
        // Get the root directory of the game (where the .exe is)
        string songFolderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Songs");

        // Check if the folder exists, if not, create it
        if (!Directory.Exists(songFolderPath))
        {
            Directory.CreateDirectory(songFolderPath);
            Debug.Log($"Created '{folderName}' folder at: {songFolderPath}");
        }
        else
        {
            Debug.Log($"'{folderName}' folder already exists at: {songFolderPath}");
        }
    }

}
