using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public PlayMode selectedMode;
    public void PlayGame(){
        Debug.Log("Play clicked");
    }
    public void SelectSinglePlay(){
        SongManager.Instance.isPlaylistMode = false;
        OpenSongSelection();
    }
    public void SelectPlaylist(){
        SongManager.Instance.isPlaylistMode = true;
        OpenSongSelection();
    }

    private void OpenSongSelection()
    {
        SceneManager.LoadSceneAsync("SongSelectionScene");
    }

    public void QuitGame(){
        Debug.Log("Game Quit");
        Application.Quit();
    }

    public void OpenOptions(){
        Debug.Log("Options clicked");
    }
    
    public void OpenCredits(){
        Debug.Log("Credits Clicked");
    }
}
