using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] private int attackDamage = 20;
    private bool attackActive = false;

    public void ActivateAttack()
    {
        attackActive = true;
        // Deactivate after a short delay (match with animation)
        Invoke("DeactivateAttack", 0.2f);
    }

    private void DeactivateAttack()
    {
        attackActive = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (attackActive && other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }
}