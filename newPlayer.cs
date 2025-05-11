using UnityEngine;
public class playercontrols : MonoBehaviour
{
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
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        // Check for attack input
        if (Input.GetKeyDown(KeyCode.K) && !isAttacking && !isHit)
        {
            isAttacking = true;
            attackTimer = 0f;
            animator.Play(attackAnim);
            // You can add attack logic here (e.g., spawn attack hitbox, etc.)
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
}