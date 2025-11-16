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

    // --- Private Components ---
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody[] ragdollRigidbodies;
    private Collider mainCollider;

    // --- Static Target ---
    [Tooltip("A static target for ALL zombies to follow. Assign this in the Inspector.")]
    public Transform goalTarget; // <-- Assign your "ZombieGoal" object to one zombie in the Inspector
    private static Transform s_goalTarget; // All zombies will share this one target

    void Awake()
    {
        // --- Get All Components ---
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        mainCollider = GetComponent<Collider>();
        
        // Find all rigidbodies that are part of the ragdoll
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();

        // --- Set the Static Goal ---
        // This makes it so you only have to set the 'goalTarget' on one zombie prefab
        if (goalTarget != null && s_goalTarget == null)
        {
            s_goalTarget = goalTarget;
        }
    }

    void Start()
    {
        // --- Activate the Zombie ---
        // 1. Turn the ragdoll "OFF" so it can walk
        SetRagdollActive(false);

        // 2. Tell the NavMeshAgent where to go
        if (s_goalTarget != null)
        {
            agent.SetDestination(s_goalTarget.position);
        }
        
        // 3. Tell the Animator to play the "Walk" animation
        animator.SetBool("isWalking", true);
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
    // This function is called automatically by Unity's physics engine
    // when another Rigidbody (the peashooter) hits this object's mainCollider.
    private void OnCollisionEnter(Collision collision)
    {
        // We only care if we are hit by a "Peashooter"
        if (collision.gameObject.CompareTag("Peashooter"))
        {
            // 1. Get the exact point of the hit
            Vector3 hitPoint = collision.GetContact(0).point;
            
            // 2. Spawn the "splat" particle effect
            if (hitEffectPrefab != null)
            {
                // W4 CONCEPT: Particle System
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
        // W4 CONCEPT: Joints (by activating the ragdoll)
        SetRagdollActive(true); 
        
        // This script is no longer needed, the physics engine takes over
        this.enabled = false; 
    }
}