using UnityEngine;

public class IceProjectile : MonoBehaviour
{
    public float iceSpeed = 20f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 direction)
    {
        rb.velocity = direction * iceSpeed;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Environment"))
        {
            Destroy(gameObject);
        }
    }
}
