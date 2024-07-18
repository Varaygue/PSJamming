using UnityEngine;

public class RaycastCamera : MonoBehaviour
{
    // Length of the ray
    public float rayLength = 10f;

    // Public variable to store the last hit point
    public Vector3 lastHitPoint;

    // Flag to check if there was a hit
    public bool hitDetected;

    // Reference to the Indicator prefab
    public GameObject IndicatorPrefab;

    // Layer mask to exclude the Indicator layer
    public LayerMask layerMask;

    // Instance of the Indicator
    private GameObject indicatorInstance;

    void Start()
    {
        // Instantiate the Indicator prefab
        indicatorInstance = Instantiate(IndicatorPrefab);

        // Set layer mask to ignore Indicator layer
        layerMask = ~LayerMask.GetMask("IndicatorLayer");
    }

    void Update()
    {
        // Define the ray starting point and direction
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Perform the raycast with layer mask
        if (Physics.Raycast(ray, out hit, rayLength, layerMask))
        {
            // Store the hit point coordinates
            lastHitPoint = hit.point;
            hitDetected = true;
        }
        else
        {
            hitDetected = false;
        }

        // Draw the ray in the editor
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);

        // Update the position of the Indicator to the last hit point
        if (indicatorInstance != null)
        {
            indicatorInstance.transform.position = lastHitPoint;
        }
    }
}
