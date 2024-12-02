using UnityEngine;

public class MoveUltra : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 60f;
    public LayerMask layerMask;
    public Color hitColor = Color.red;
    public float rayDistance = 2f;

    private Color originalColor;
    private GameObject lastHitObject;

    void Update()
    {
        // Movement
        float moveDirection = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float turnDirection = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;

        // Rotate the object based on the left/right arrow keys
        transform.Rotate(Vector3.up * turnDirection);

        // Move the object forward or backward based on the object's current forward direction
        transform.Translate(Vector3.forward * moveDirection);

        // Raycasting for falling and surface alignment
        AlignToSurface();
    }

    void AlignToSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            // Adjust position
            transform.position = hit.point + transform.up * 0.5f;

            // Adjust rotation to align with surface normal
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            // Change color of the hit surface if it's closer
            ChangeHitObjectColor(hit, true);
        }
        else
        {
            ResetLastHitObject();
        }
    }

    void ChangeHitObjectColor(RaycastHit hit, bool isAligningSurface)
    {
        Debug.Log("Collided with: " + hit.collider.name);

        Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
        if (hitRenderer != null)
        {
            if (lastHitObject != hit.collider.gameObject)
            {
                ResetLastHitObject();
                lastHitObject = hit.collider.gameObject;
                originalColor = hitRenderer.material.color;
                hitRenderer.material.color = hitColor;
            }
        }
        else if (isAligningSurface && lastHitObject == hit.collider.gameObject)
        {
            // Keep the color changed if aligning to the same surface
            Renderer lastHitRenderer = lastHitObject.GetComponent<Renderer>();
            if (lastHitRenderer != null)
            {
                lastHitRenderer.material.color = hitColor;
            }
        }
    }

    void ResetLastHitObject()
    {
        if (lastHitObject != null)
        {
            Renderer lastHitRenderer = lastHitObject.GetComponent<Renderer>();
            if (lastHitRenderer != null)
            {
                lastHitRenderer.material.color = originalColor;
            }
            lastHitObject = null;
        }
    }
}
