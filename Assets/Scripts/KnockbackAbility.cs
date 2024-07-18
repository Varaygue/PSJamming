using UnityEngine;
using System.Collections;

public class KnockbackAbility : MonoBehaviour
{
    public float knockbackForce = 10f; // Adjust as needed
    public float knockbackDuration = 0.2f; // Duration of knockback effect

    private void OnTriggerEnter(Collider other)
    {
        // Check if collided with an enemy
        if (other.CompareTag("Enemy"))
        {
            // Calculate knockback direction away from the player
            Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
            knockbackDirection.y = 0; // Optional: Prevent knockback in the vertical direction

            // Apply knockback force
            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();
            if (enemyRigidbody != null)
            {
                enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                
                // Optionally, disable enemy movement temporarily
                StartCoroutine(DisableMovement(enemyRigidbody));
            }
        }
    }

    private IEnumerator DisableMovement(Rigidbody enemyRigidbody)
    {
        // Disable movement temporarily during knockback
        enemyRigidbody.velocity = Vector3.zero;
        yield return new WaitForSeconds(knockbackDuration);
        // Re-enable movement after knockback duration
    }
}
