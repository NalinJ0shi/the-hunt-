using UnityEngine;

public class playercontrols : Singleton<playercontrols>{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.down;
    private bool isAttacking = false;
    private bool isHit = false;

    // Animation state names - updated for new animations
    private readonly int idleAnim = Animator.StringToHash("idle");
    private readonly int walkAnim = Animator.StringToHash("walk");
    private readonly int attackAnim = Animator.StringToHash("attack");
    private readonly int hitAnim = Animator.StringToHash("hit");

    // Timer for attack animation
    private float attackTimer = 0f;
    private float attackDuration = 0.5f; // Adjust based on your attack animation length

    // Timer for hit animation
    private float hitTimer = 0f;
    private float hitDuration = 0.5f; // Adjust based on your hit animation length

    // Attack properties
    [Header("Attack Properties")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private Transform attackPoint; // Create an empty GameObject child for this
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private LayerMask enemyLayers; // Set this in the Inspector
    [SerializeField] private PlayerAttackHitbox attackHitbox;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        // Fix rotation issue by freezing Z rotation
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Create attack point if not assigned
        if (attackPoint == null)
        {
            GameObject newAttackPoint = new GameObject("AttackPoint");
            newAttackPoint.transform.parent = transform;
            newAttackPoint.transform.localPosition = new Vector3(0.5f, 0, 0); // Offset to the right
            attackPoint = newAttackPoint.transform;
        }
    }

    private void Update()
    {
        // Get input
        movement = Vector2.zero;
        // Only process movement input if not attacking or being hit
        if (!isAttacking && !isHit)
        {
            if (Input.GetKey(KeyCode.A)) movement.x = -1;
            if (Input.GetKey(KeyCode.D)) movement.x = 1;
            if (Input.GetKey(KeyCode.W)) movement.y = 1;
            if (Input.GetKey(KeyCode.S)) movement.y = -1;
            if (movement.magnitude > 1f)
                movement.Normalize();
            // Update last direction when moving
            if (movement.magnitude > 0.1f)
                lastDirection = movement;
                
            // Flip sprite based on horizontal movement
            if (movement.x < 0)
                spriteRenderer.flipX = true;
            else if (movement.x > 0)
                spriteRenderer.flipX = false;
        }

        // Update attack point position based on last direction
        UpdateAttackPointPosition();

        // Check for attack input
        if (Input.GetKeyDown(KeyCode.K) && !isAttacking && !isHit)
        {
            Debug.Log("[PLAYER] Attack key pressed");
            isAttacking = true;
            attackTimer = 0f;
            animator.Play(attackAnim);
            
            // Activate attack hitbox
            if (attackHitbox != null)
            {
                Debug.Log("[PLAYER] Activating attack hitbox");
                attackHitbox.ActivateAttack();
            }
            else
            {
                Debug.Log("[PLAYER] No attackHitbox assigned");
            }
            
            // Detect hits
            Debug.Log($"[PLAYER] Checking for enemies at position {attackPoint.position} with radius {attackRadius}");
            Debug.Log($"[PLAYER] Using enemyLayers value: {enemyLayers.value}");
            
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);
            Debug.Log($"[PLAYER] Found {hitEnemies.Length} enemy colliders in attack area");
            
            // Apply damage to enemies
            foreach (Collider2D enemy in hitEnemies)
            {
                Debug.Log($"[PLAYER] Processing hit on enemy: {enemy.name}");
                
                // Try to get the enemy health component first
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    Debug.Log($"[PLAYER] Found EnemyHealth component, applying {attackDamage} damage");
                    enemyHealth.TakeDamage(attackDamage);
                }
                else
                {
                    Debug.Log("[PLAYER] No EnemyHealth component found on enemy");
                    
                    // Fallback to the enemy controller if no health component
                    EnemyController enemyController = enemy.GetComponent<EnemyController>();
                    if (enemyController != null)
                    {
                        Debug.Log("[PLAYER] Found EnemyController, applying damage through controller");
                        enemyController.TakeDamage(attackDamage);
                    }
                    else
                    {
                        Debug.LogError($"[PLAYER] Enemy {enemy.name} has no EnemyHealth or EnemyController component!");
                    }
                }
            }
        }

        // Handle attack animation timing
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDuration)
            {
                isAttacking = false;
            }
        }

        // Handle hit animation timing
        if (isHit)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer >= hitDuration)
            {
                isHit = false;
            }
        }

        // Set animation state based on player state
        UpdateAnimation();
    }

    private void UpdateAttackPointPosition()
    {
        if (attackPoint == null) return;
        
        // Position the attack point based on the direction the player is facing
        Vector3 offset = Vector3.zero;
        
        if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
        {
            // Horizontal direction is dominant
            offset.x = lastDirection.x > 0 ? 0.8f : -0.8f;
        }
        else
        {
            // Vertical direction is dominant
            offset.y = lastDirection.y > 0 ? 0.8f : -0.8f;
        }
        
        attackPoint.localPosition = offset;
    }

    private void UpdateAnimation()
    {
        bool isMoving = movement.magnitude > 0.1f;
        // Priority: hit > attack > walk > idle
        if (isHit)
        {
            animator.Play(hitAnim);
        }
        else if (isAttacking)
        {
            animator.Play(attackAnim);
        }
        else if (isMoving)
        {
            animator.Play(walkAnim);
        }
        else
        {
            animator.Play(idleAnim);
        }
    }

    // Public method to trigger hit animation
    public void TakeHit()
    {
        isHit = true;
        hitTimer = 0f;
        animator.Play(hitAnim);
    }

    private void FixedUpdate()
    {
        // Only move the player if not attacking or being hit
        if (!isAttacking && !isHit)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
    
    // Draw attack radius in the editor for visualization
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}