using UnityEngine;

public class Note : MonoBehaviour
{
    public float fallSpeed = 5f;  // Speed at which the note falls
    
    private bool canHit = false;
    private GameManager gameManager;

    void Start(){
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
    void Update()
    {
        // Move the note down
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Destroy the note if it goes off screen
        if (transform.position.y < -10f)
        {
            if(gameManager != null){
                gameManager.combo = 0;
                gameManager.comboText.text = "";
            }
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pad"))
        {
            canHit = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Pad"))
        {
            canHit = false;
        }
    }

    public bool IsHittable()
    {
        return canHit;
    }
}