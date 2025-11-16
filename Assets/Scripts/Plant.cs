using UnityEngine;

// Class name "Plant" must match the file name "Plant.cs"
public class Plant : MonoBehaviour
{
    [Header("Tower Stats")]
    [Tooltip("How far the plant can see enemies.")]
    public float detectionRange = 15f;

    [Tooltip("How many shots per second.")]
    public float fireRate = 1f; 

    [Tooltip("The tag your enemies have.")]
    public string enemyTag = "Enemy";

    [Header("Setup (Drag-and-Drop)")]
    [Tooltip("Drag your projectile prefab here.")]
    public GameObject projectilePrefab; // Your existing projectile

    [Tooltip("The empty object at the plant's mouth.")]
    public Transform firePoint;

    [Header("Homework Requirement")]
    [Tooltip("The power of the shot.")]
    public float launchForce = 1000f;

    // --- Private Variables ---
    private Transform target;
    private float fireCountdown = 0f;

    void Start()
    {
        // This tells Unity to run the "UpdateTarget" function
        // every 0.5 seconds. This is better for performance.
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget()
    {
        // 1. Create a detection sphere
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        // 2. Loop through everything it found
        foreach (Collider col in hitColliders)
        {
            // 3. Check if it's an enemy
            if (col.CompareTag(enemyTag))
            {
                // 4. Check if it's the closest one
                float distanceToEnemy = Vector3.Distance(transform.position, col.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = col.gameObject;
                }
            }
        }

        // 5. Set our target
        if (nearestEnemy != null)
            target = nearestEnemy.transform;
        else
            target = null;
    }

    void Update()
    {
        // If we don't have a target, do nothing.
        if (target == null)
            return;

        // --- Aim at target ---
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        // Only rotate on the Y-axis (left and right)
        transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);

        // --- Fire Cooldown ---
        if (fireCountdown <= 0f)
        {
            Fire();
            fireCountdown = 1f / fireRate; // Reset cooldown
        }
        fireCountdown -= Time.deltaTime;
    }

    void Fire()
    {
        // 1. Create a copy of your projectile
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 2. Get its Rigidbody
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        // 3. Calculate direction to the target
        Vector3 direction = (target.position - firePoint.position).normalized;

        // 4. SHOOT! (This is the homework requirement)
        rb.AddForce(direction * launchForce);
    }
}