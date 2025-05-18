using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    // References
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    
    // State Management
    private enum EnemyState { Idle, Chasing, Attacking, Hurt, Dead }
    private EnemyState currentState = EnemyState.Idle;
    private int currentHealth;
    private float lastAttackTime;
    private bool canAttack = true;
    private bool isAttacking = false;

    // Animation State Names - Changed to match actual animation names
    private string idleAnim = "idle";
    private string walkAnim = "walk";
    private string attack1Anim = "attack1";
    private string attack2Anim = "attack2";
    private string hurtAnim = "hurt";
    private string deathAnim = "death"; // FIXED: Match actual animation name

    // Animation Durations
    private float attack1Duration = 0.6f;
    private float attack2Duration = 0.7f;
    private float hurtDuration = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Find player by tag
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        // Set up physics
        rb.gravityScale = 0f;
        // Prevent rotation with both settings
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        currentHealth = maxHealth;
        
        // Debug animation clips available
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        Debug.Log("Available animation clips:");
        foreach (AnimationClip clip in clips)
        {
            Debug.Log($"Clip name: {clip.name}");
        }
    }

    // Make sure Update keeps running animations even when dead
    private void Update()
    {
        // Ensure rotation stays at zero
        transform.rotation = Quaternion.identity;
        
        // Get player distance if we have a player
        float distanceToPlayer = 0;
        if (player != null)
        {
            distanceToPlayer = Vector2.Distance(transform.position, player.position);
        }
        
        // Even when dead, keep updating animations
        UpdateAnimation();
        
        // Skip further updates if dead or no player
        if (currentState == EnemyState.Dead || player == null)
            return;
            
        // Debug with F1 key
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"Distance: {distanceToPlayer}, State: {currentState}, CanAttack: {canAttack}, IsAttacking: {isAttacking}");
            Debug.Log($"Animation states: idle={idleAnim}, walk={walkAnim}, attack1={attack1Anim}, attack2={attack2Anim}");
            Debug.Log($"Current animator state: {animator.GetCurrentAnimatorStateInfo(0).ToString()}");
            
            // List all animation clips
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            string clipNames = "Animation clips: ";
            foreach (AnimationClip clip in clips)
            {
                clipNames += clip.name + ", ";
            }
            Debug.Log(clipNames);
        }
        
        // Handle states based on distance
        if (!isAttacking && currentState != EnemyState.Hurt && currentState != EnemyState.Dead)
        {
            if (distanceToPlayer <= attackRange && canAttack)
            {
                // Attack if in range and can attack
                StartCoroutine(PerformAttack());
            }
            else if (distanceToPlayer <= detectionRange)
            {
                // Chase if in detection range but not in attack range
                currentState = EnemyState.Chasing;
            }
            else
            {
                // Stand idle if player is out of detection range
                currentState = EnemyState.Idle;
            }
        }
        
        // Update sprite direction
        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        
        // Update animation
        UpdateAnimation();
        
        // Handle attack cooldown
        if (!canAttack && Time.time - lastAttackTime >= attackCooldown)
        {
            canAttack = true;
        }
    }

    private void FixedUpdate()
    {
        if (currentState != EnemyState.Chasing || player == null || 
            isAttacking || currentState == EnemyState.Dead || currentState == EnemyState.Hurt)
            return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Only move if we're in detection range but outside attack range
        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            
            // Ensure rotation remains at zero
            rb.rotation = 0f;
        }
    }

    private void UpdateAnimation()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                animator.Play(idleAnim);
                break;
            case EnemyState.Chasing:
                animator.Play(walkAnim);
                break;
            case EnemyState.Attacking:
                // Don't update attack animation here - it's handled in the attack coroutine
                // This prevents animation interruption
                break;
            case EnemyState.Dead:
                animator.Play(deathAnim);
                break;
        }
    }

    private IEnumerator PerformAttack()
    {
        // Set flags to prevent multiple attacks
        isAttacking = true;
        canAttack = false;
        lastAttackTime = Time.time;
        currentState = EnemyState.Attacking;
        
        // Choose between attack1 and attack2
        bool useAttack2 = Random.value > 0.5f;
        float attackDuration = useAttack2 ? attack2Duration : attack1Duration;
        
        // Play attack animation
        Debug.Log("Playing attack animation: " + (useAttack2 ? "attack2" : "attack1"));
        animator.Play(useAttack2 ? attack2Anim : attack1Anim);
        
        // Wait for the right moment in animation to apply damage
        yield return new WaitForSeconds(attackDuration * 0.5f);
        
        // Apply damage if player is still in range
        if (player != null)
        {
            float currentDistance = Vector2.Distance(transform.position, player.position);
            if (currentDistance <= attackRange)
            {
                playercontrols playerController = player.GetComponent<playercontrols>();
                if (playerController != null)
                {
                    playerController.TakeHit();
                    // Get player health component and apply damage
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                    }
                }
            }
        }
        
        // Wait for attack animation to finish
        yield return new WaitForSeconds(attackDuration * 0.5f);
        
        // Reset attack flag
        isAttacking = false;
        
        // Return to appropriate state
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                currentState = EnemyState.Chasing;
            }
            else
            {
                currentState = EnemyState.Idle;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead)
            return;
            
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(TakeHit());
        }
    }

    private IEnumerator TakeHit()
    {
        currentState = EnemyState.Hurt;
        animator.Play(hurtAnim);
        
        yield return new WaitForSeconds(hurtDuration);
        
        if (currentState != EnemyState.Dead)
        {
            // Return to appropriate state
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                }
                else
                {
                    currentState = EnemyState.Idle;
                }
            }
        }
    }

    private void Die()
    {
        currentState = EnemyState.Dead;
        animator.Play("death");
        
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;
        Destroy(gameObject, 3f);
    }

    // Visualize detection and attack ranges in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    // Handle collisions to prevent rotation
    private void OnCollisionEnter2D(Collision2D collision) 
    {
        // Reset rotation on collision
        transform.rotation = Quaternion.identity;
        rb.rotation = 0f;
    }
}