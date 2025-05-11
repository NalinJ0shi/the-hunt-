using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;
    
    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }
        
        // Get positions as Vector2 to ignore Z-axis
        Vector2 enemyPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPosition = new Vector2(player.position.x, player.position.y);
        
        // Calculate actual distance
        float distance = Vector2.Distance(enemyPosition, playerPosition);
        Debug.Log("True distance: " + distance);
        
        // Move toward player on X and Y only
        Vector3 newPosition = Vector2.MoveTowards(enemyPosition, playerPosition, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }
}