using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    public float fireSpeed = 20f;
    private Rigidbody rb;
    public GameObject projectileObject;
    public GameObject areaOfDamage;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 direction)
    {
        rb.velocity = direction * fireSpeed;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Environment"))
        {
            Destroy(projectileObject);
            rb.velocity=Vector3.zero;
            areaOfDamage.SetActive(true);
        }

        if(other.CompareTag("Enemy"))
        {
            Destroy(projectileObject);
            Destroy(other.gameObject);
            rb.velocity=Vector3.zero;
            areaOfDamage.SetActive(true);
        }
    }
}
