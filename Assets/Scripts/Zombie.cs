using UnityEngine;
using UnityEngine.AI; // IMPORTANT: You need this for the NavMeshAgent

[RequireComponent(typeof(NavMeshAgent))] // Automatically adds the NavMeshAgent
[RequireComponent(typeof(Animator))]
public class Zombie : MonoBehaviour
{
    [Header("Zombie Stats")]
    [Tooltip("How much health the zombie has.")]
    public int health = 100;
    [Tooltip("The damage the peashooter will do.")]
    public int damageToTake = 25;

    [Header("Effects")]
    [Tooltip("Drag your 'splat' particle effect prefab here")]
    public GameObject hitEffectPrefab;

    [Header("Wandering")]
    [Tooltip("How far the zombie will look for a new random point.")]
    public float wanderRadius = 15f;
    [Tooltip("How long the zombie waits (at minimum) before finding a new point.")]
    public float wanderTimer = 5f;

    // --- Private Components & Timers ---
    private float timer; // Timer to count up
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody[] ragdollRigidbodies;
    private Collider mainCollider;

    void Awake()
    {
        // --- Get All Components ---
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        mainCollider = GetComponent<Collider>();

        // Find all rigidbodies that are part of the ragdoll
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    void Start()
    {
        // --- Activate the Zombie ---
        // 1. Turn the ragdoll "OFF" so it can walk
        SetRagdollActive(false);

        // 2. Teleport to the nearest valid NavMesh point (THIS FIXES THE SPAWN ERROR)
        // ---- THIS IS CORRECTION #1 ----
        // The 'S{' from the previous code was a typo. It's now just '{'.
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            // If we can't find a spot, there's a bigger problem.
            Debug.LogError("COULD NOT FIND NAVMESH FOR ZOMBIE!", this);
            Destroy(gameObject); // Destroy this zombie, it can't move.
            return;
        }

        // 3. Tell the Animator to play the "Walk" animation
        animator.SetBool("isWalking", true);

        // 4. Set the timer and find the first wander spot
        timer = wanderTimer;
        FindNewWanderPoint();
    }

    void Update()
    {
        // If the agent is not enabled, do nothing (e.g., it's dead)
        if (!agent.enabled) return;

        // --- Wander Logic ---
        timer += Time.deltaTime;

        // 1. If the timer runs out OR the zombie reached its destination...
        if (timer >= wanderTimer || agent.remainingDistance <= agent.stoppingDistance)
        {
            // ...find a new place to walk.
            FindNewWanderPoint();
        }
    }

    /// <summary>
    /// Finds a new random point on the NavMesh and tells the agent to go there.
    /// </summary>
    void FindNewWanderPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        // Find the closest point on the NavMesh to our random spot
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            // Set the agent's new destination
            agent.SetDestination(hit.position);
        }

        // Reset the timer
        timer = 0;
    }

    /// <summary>
    /// This is the master function to switch between "walking" and "ragdoll".
    /// </summary>
    /// <param name="isActive">True = ragdoll on, False = ragdoll off</param>
    void SetRagdollActive(bool isActive)
    {
        // The Animator and NavMeshAgent must be OFF for the ragdoll to work
        animator.enabled = !isActive;
        agent.enabled = !isActive;

        // The main collider (for walking) must be OFF when the ragdoll is ON
        mainCollider.enabled = !isActive;

        // Loop through all ragdoll parts (limbs, head, etc.)
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            // isKinematic = true means "physics doesn't move this" (ragdoll OFF)
            // isKinematic = false means "physics DOES move this" (ragdoll ON)
            rb.isKinematic = !isActive;
        }
    }

    // --- THIS IS THE HOMEWORK REQUIREMENT: OnCollisionEnter ---
    private void OnCollisionEnter(Collision collision)
    {
        // We only care if we are hit by a "Peashooter"
        if (collision.gameObject.CompareTag("Peashooter"))
        {
            // 1. Get the exact point of the hit
            Vector3 hitPoint = collision.GetContact(0).point;

            // 2. Spawn the "splat" particle effect
            // ---- THIS IS CORRECTION #2 ----
            // The 'prefab' variable didn't exist. We check for 'null' instead.
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
            }

            // 3. Apply damage
            health -= damageToTake;

            // 4. Destroy the peashooter
            Destroy(collision.gameObject);

            // 5. Check if the zombie should die
            if (health <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        // --- THIS IS THE "WOW" MOMENT ---
        SetRagdollActive(true);

        // This script is no longer needed, the physics engine takes over
        this.enabled = false;
    }
}