using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI feverText;
    public MovePad playerPad; // The pad controlled by the player
    public int baseScorePerHit = 100;
    private int score = 0;
    public int combo = 0;
    public int maxCombo = 0;
    public float feverMax = 100f;
    public float feverAmount = 0f;
    public float feverDuration = 5f;
    public int feverMultiplier = 2;
    public float feverBuildPerHit = 4f;
    private bool isFeverActive = false;
    private float feverTimer = 0f;
    public float hitWindow = 2f;
    public Slider feverSlider;
    private PlayerInputActions playerInputActions;
    public ParticleSystem destry;
    private void Start()
    {
        string songTitle = SongManager.Instance.selectedSongTitle;
        score = 0;
        combo = 0;
        UpdateScoreText();
        UpdateComboText();
        feverText.text = "";
        feverSlider.value = 0;
        feverSlider.maxValue = feverMax;
        feverSlider.fillRect.GetComponent<Image>().color = new Color(0.8f, 1f, 1f, 0.7f);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // Enable the input actions
        playerInputActions.Enable();

        // Subscribe to the hit action
        playerInputActions.Player.Hit.performed += OnHitPerformed;
        playerInputActions.Player.Fever.performed += OnFeverActivated;
    }

    private void OnDisable()
    {
        // Disable the input actions
        playerInputActions.Disable();

        // Unsubscribe from the hit action
        playerInputActions.Player.Hit.performed -= OnHitPerformed;
        playerInputActions.Player.Fever.performed -= OnFeverActivated;
    }

    void Update()
    {
        if (isFeverActive){
            feverTimer -= Time.deltaTime;
            feverAmount = feverMax*feverTimer/feverDuration;
            UpdateFeverSlider();
            if(feverTimer <= 0f){
                EndFeverMode();
            }
        }
        else if (feverAmount >= feverMax){
            feverText.text = "Space Bar!";
        } 

        if (score > SongManager.Instance.finalScore){
            SongManager.Instance.finalScore = score;
            }
        if(combo > SongManager.Instance.maxCombo){
            SongManager.Instance.maxCombo = combo;
        }
    }
    private void OnHitPerformed(InputAction.CallbackContext context)
    {
        CheckForHit();
    }

    private void OnFeverActivated(InputAction.CallbackContext context)
    {
        if (feverAmount >= feverMax && !isFeverActive){
            ActivateFeverMode();
        }
    }

    void CheckForHit()
    {
        // Get the position of the player pad
        Vector3 padPosition = playerPad.transform.position;

        // Use a radius to check for nearby notes
        Collider2D[] hitNotes = Physics2D.OverlapCircleAll(padPosition,hitWindow);

        bool noteHit = false;
        
        foreach (var note in hitNotes)
        {
            // Make sure the object is a note by checking if it has a Note script
            Note noteScript = note.GetComponent<Note>();
            if (noteScript != null)
            {
                int baseScore = baseScorePerHit + (baseScorePerHit * (combo /25));
                int finalScore = isFeverActive ? baseScore * feverMultiplier : baseScore;
                // Successful hit: Increase score and combo
                score += finalScore; // Combo multiplier affects score growth
                combo += 1;
                feverAmount = Mathf.Min(feverAmount + feverBuildPerHit, feverMax);
                UpdateFeverSlider();
                noteHit = true;
                // Update score and combo texts
                UpdateScoreText();
                UpdateComboText();
                ParticleSystem newParticle = Instantiate(destry, note.transform.position, Quaternion.identity);
                newParticle.Play();

                // Destroy the particle system after it finishes playing
                Destroy(newParticle.gameObject, newParticle.main.duration);
                //Debug.Log("Hit! Score: " + score);

                // Destroy the note
                Destroy(note.gameObject);

            }
        }

        // If no notes were hit, reset combo
        if (!noteHit)
        {
            ResetCombo();
        }
    }

    // Reset combo to 0 when a note is missed
    void ResetCombo()
    {
        combo = 0;
        UpdateComboText();
        //Debug.Log("Miss! Combo reset.");
    }

    // Update the score text
    void UpdateScoreText()
    {
        scoreText.text = score.ToString() + " POINTS";
    }

    // Update the combo text
    void UpdateComboText()
    {
        // Update the combo text
        comboText.text = combo > 0 ? "x" + combo.ToString() : "";

        // Update Max Combo
        if (combo > maxCombo)
        {
            maxCombo = combo;
        }
        StartCoroutine(ComboTextEffect()); // Trigger effect
    }
    
    void ActivateFeverMode()
    {
        isFeverActive = true;
        feverTimer = feverDuration;
        feverText.text = "";
        //feverAmount = 0f;
        //UpdateFeverSlider();
        scoreText.color = new Color(0.5f, 1f, 0.5f);
        feverSlider.fillRect.GetComponent<Image>().color = new Color(0.5f, 1f, 0.5f,0.7f);
        //feverText.text = "Fever Active!";
        //Debug.Log("fever activated");
    }
    void EndFeverMode()
    {
        isFeverActive = false;
        feverAmount = 0f;
        feverSlider.fillRect.GetComponent<Image>().color = new Color(0.8f, 1f, 1f, 0.7f);
        scoreText.color = Color.white;
        UpdateFeverSlider();
        //feverText.text = "Fever Inactive";
        //Debug.Log("fever inactive");
    }

    void UpdateFeverSlider(){
        feverSlider.value = feverAmount;
    }
    IEnumerator ComboTextEffect() 
    {
        comboText.transform.localScale = Vector3.one * 1.2f; // Scale up
        comboText.color = Color.yellow; // Change color
        yield return new WaitForSeconds(0.1f);
        comboText.transform.localScale = Vector3.one; // Scale back down
        comboText.color = Color.white; // Restore original color
    }

    public int GetScore(){
        return score;
    }

    public IEnumerator DelayBeforeResultScreen(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for 'delay' seconds

        UnityEngine.SceneManagement.SceneManager.LoadScene("ResultScreen"); // Load the result screen
    }

    public IEnumerator DelayBeforeNextSong(float delay){
        yield return new WaitForSeconds(delay);
        
        SongManager.Instance.MoveToNextSong();
    }
}
