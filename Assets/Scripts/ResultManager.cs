using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI Score;
    public TextMeshProUGUI Status;
    public TextMeshProUGUI MaxCombo;
    public TextMeshProUGUI songTitle;

    public Button ContinueButton;
    // Start is called before the first frame update
    void Start()
    {
        songTitle.text = SongManager.Instance.selectedSongTitle.ToString();
        Score.text = "Score: " + SongManager.Instance.finalScore.ToString();
        MaxCombo.text = "Max Combo: " + SongManager.Instance.maxCombo.ToString();
        Status.text = "Status: " + GetStatus();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   string GetStatus()
    {
        if (SongManager.Instance.pass)
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

