using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    public float fireSpeed = 20f;
    private Rigidbody rb;
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
            rb.velocity=Vector3.zero;
            areaOfDamage.SetActive(true);
        }
    }
}
