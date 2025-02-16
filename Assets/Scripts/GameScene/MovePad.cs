using UnityEngine;
using UnityEngine.InputSystem;

public class MovePad : MonoBehaviour
{
    public float yOffset = -3.5f; // Adjust this value based on your setup
    public float leftBoundary = -10f; // Set the left boundary
    public float rightBoundary = 10f; // Set the right boundary

    private void Update()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));
        
        // Clamp the x position within the boundaries
        float clampedX = Mathf.Clamp(worldPosition.x, leftBoundary, rightBoundary);

        // Update the pad's position with clamped x and fixed yOffset
        transform.position = new Vector3(clampedX, yOffset, transform.position.z);
    }
}
