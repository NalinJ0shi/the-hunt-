using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 0.5f;
    
    private bool isInvincible = false;
    private EnemyController enemyController;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        enemyController = GetComponent<EnemyController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        Debug.Log($"[ENEMY_HEALTH] Initialized with maxHealth: {maxHealth}");
        if (enemyController == null)
            Debug.LogError("[ENEMY_HEALTH] No EnemyController component found!");
        if (spriteRenderer == null)
            Debug.LogError("[ENEMY_HEALTH] No SpriteRenderer component found!");
    }
    
    public void TakeDamage(int damage)
    {
        Debug.Log($"[ENEMY_HEALTH] TakeDamage called with damage: {damage}");
        
        if (isInvincible)
        {
            Debug.Log("[ENEMY_HEALTH] Enemy is invincible, ignoring damage");
            return;
        }
        
        int oldHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        Debug.Log($"[ENEMY_HEALTH] Health reduced from {oldHealth} to {currentHealth}");
        
        // Visual feedback
        StartCoroutine(DamageFlash());
        
        // Check for death
        if (currentHealth <= 0)
        {
            Debug.Log("[ENEMY_HEALTH] Enemy health <= 0, calling Die()");
            Die();
        }
        else
        {
            // Notify enemy controller about damage
            if (enemyController != null)
            {
                Debug.Log("[ENEMY_HEALTH] Notifying EnemyController about damage");
                enemyController.TakeDamage(damage);
            }
            else
            {
                Debug.LogError("[ENEMY_HEALTH] enemyController is null!");
            }
            
            // Start invincibility period
            StartCoroutine(InvincibilityFrames());
        }
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
        // Notify enemy controller about death
        if (enemyController != null)
        {
            enemyController.TakeDamage(currentHealth + 1); // Force death state
        }
        
        Debug.Log("Enemy died!");
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