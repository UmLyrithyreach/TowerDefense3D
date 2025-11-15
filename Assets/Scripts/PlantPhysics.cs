using UnityEngine;

public class PlantPhysics : MonoBehaviour
{
    [Header("Homework Requirements")]
    [Tooltip("Drag your 'Cannonball' prefab here")]
    public GameObject projectilePrefab;

    [Tooltip("The target the tower will aim at")]
    public Transform targetEnemy;

    [Tooltip("The point where the cannonball will spawn")]
    public Transform firePoint;

    [Tooltip("Adjusts the power of the shot")]
    public float launchForce = 1000f;

    // Update is called once per frame
    void Update()
    {
        // This is the "Respond to user input" requirement
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    void Fire()
    {
        if (projectilePrefab == null || targetEnemy == null || firePoint == null)
        {
            Debug.LogWarning("Tower is not set up correctly. Missing prefab, target, or fire point.");
            return;
        }

        // 1. Create the projectile
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 2. Get its Rigidbody
        Rigidbody rb = projectileInstance.GetComponent<Rigidbody>();

        // 3. Calculate direction
        Vector3 direction = (targetEnemy.position - firePoint.position).normalized;

        // 4. This is the "Apply movement using AddForce()" requirement
        rb.AddForce(direction * launchForce);

        Debug.Log("Fired projectile with " + launchForce + " force!");
    }
}