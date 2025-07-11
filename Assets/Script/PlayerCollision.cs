using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player collides with an object tagged as "Enemy"
        if (collision.CompareTag("Coin"))
        {
            // Log a message to the console
            Debug.Log("Hit Coin!");
            // Optionally, you can add more logic here, like reducing health or triggering a game over
        }
    }
}
