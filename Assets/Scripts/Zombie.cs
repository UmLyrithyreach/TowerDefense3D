using UnityEngine;

public class Zombie : MonoBehaviour
{
    // This function is called automatically by Unity's physics engine
    // when another collider with a Rigidbody hits this one.
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object that hit us has the "Projectile" tag
        if (collision.gameObject.CompareTag("Projectile"))
        {
            // If it's a projectile, print a message to the console
            Debug.Log("Enemy was hit by a projectile!");

            // Destroy the projectile that hit us
            Destroy(collision.gameObject);

            // Optional: Destroy the enemy too
            // Destroy(gameObject);
        }
    }
}