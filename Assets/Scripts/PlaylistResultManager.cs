using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistResultManager: MonoBehaviour
{
    public List<TextMeshProUGUI> Score = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> Status = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> MaxCombo = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> songTitle = new List<TextMeshProUGUI>();

    public Button ContinueButton;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0;i<SongManager.Instance.playlistSongPaths.Count ; i++)
        {
        songTitle[i].text = SongManager.Instance.playlistSongTitles[i].ToString();
        Score[i].text = "Score: " + SongManager.Instance.playlistScores[i].ToString();
        MaxCombo[i].text = "Max Combo: " + SongManager.Instance.playlistMaxCombos[i].ToString();
        Status[i].text = "Status: " + GetStatus(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   string GetStatus(int count)
    {        
        if (SongManager.Instance.playlistPass[count])
        {
            return "Success";
        }
        else{
            return "Failed";
        }
    }

    public void ContinueButtonOnClick(){
            UnityEngine.SceneManagement.SceneManager.LoadScene("SongSelectionScene");
    }
}

