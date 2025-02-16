using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    public float autoTransitionDelay = 10f;
    // Start is called before the first frame update
    private void Start()
    {
        if (autoTransitionDelay > 0){
            Invoke("LoadMainMenu", autoTransitionDelay);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.anyKeyDown){
            LoadMainMenu();
        }
    }
    private void LoadMainMenu(){
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
