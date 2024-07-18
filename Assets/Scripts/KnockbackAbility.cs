using UnityEngine;
using System.Collections;

public class KnockbackAbility : MonoBehaviour
{
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
            knockbackDirection.y = 0;

            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();
            if (enemyRigidbody != null)
            {
                enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                StartCoroutine(DisableMovement(enemyRigidbody));
            }
        }
    }

    private IEnumerator DisableMovement(Rigidbody enemyRigidbody)
    {
        enemyRigidbody.velocity = Vector3.zero;
        yield return new WaitForSeconds(knockbackDuration);
    }
}
