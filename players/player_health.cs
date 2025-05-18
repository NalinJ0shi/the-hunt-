using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float healthRegenRate = 0f;  // Set > 0 for health regeneration
    
    private bool isInvincible = false;
    private Animator animator;
    private playercontrols playerController;
    private SpriteRenderer spriteRenderer;
    
    // Animation state name for death
    private readonly int deathAnim = Animator.StringToHash("death");
    private bool isDead = false;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        playerController = GetComponent<playercontrols>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        // Optional health regeneration
        if (healthRegenRate > 0 && currentHealth < maxHealth && !isDead)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + (int)(healthRegenRate * Time.deltaTime));
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Visual feedback
        StartCoroutine(DamageFlash());
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility period
            StartCoroutine(InvincibilityFrames());
        }
        
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }
    
    private IEnumerator DamageFlash()
    {
        // Flash red when taking damage
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = Color.white;
    }
    
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        // Visual feedback for invincibility
        float flashDuration = 0.1f;
        int flashCount = Mathf.FloorToInt(invincibilityDuration / (flashDuration * 2));
        
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashDuration);
        }
        
        isInvincible = false;
    }
    
    private void Die()
    {
        isDead = true;
        
        // Disable player control
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Play death animation
        animator.Play(deathAnim);
        
        Debug.Log("Player died!");
        
        // Additional death logic (game over, respawn, etc.)
    }
    
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"Player healed {amount} health. Health: {currentHealth}/{maxHealth}");
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}