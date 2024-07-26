using UnityEngine;

public class RaycastCamera : MonoBehaviour
{
    public float rayLength = 10f;
    public Vector3 lastHitPoint;
    public bool hitDetected;
    public LayerMask layerMask;
    private GameObject indicatorInstance;

    void Start()
    {
        layerMask = ~LayerMask.GetMask("IndicatorLayer");
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayLength, layerMask))
        {
            lastHitPoint = hit.point;
            hitDetected = true;
        }
        else
        {
            hitDetected = false;
        }
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);

        if (indicatorInstance != null)
        {
            indicatorInstance.transform.position = lastHitPoint;
        }
    }
}
