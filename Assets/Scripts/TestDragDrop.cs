using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDragDrop : MonoBehaviour
{
   public LayerMask groundLayer;    // Assign the layer for the ground
    public LayerMask dragtopLayer;   // Assign the layer named "dragtop"
    public float raycastDistance = 100f; // The max distance for raycasting
    public float moveSpeed = 10f;    // Speed for moving the object while dragging
    public float fallSpeed = 5f;     // Speed at which the object falls to the ground
    public bool dropAtStart = false; // Toggle for whether to drop the object at the start

    private Camera mainCamera;
    private bool isDragging = false;
    private bool isFalling = false;
    private Vector3 offset;
    private Renderer objRenderer;    // Renderer component to get the object's bounds
    private Collider objCollider;    // Collider of the object to ignore during raycasts

    void Start()
    {
        mainCamera = Camera.main;
        objRenderer = GetComponent<Renderer>(); // Get the Renderer component for bounds
        objCollider = GetComponent<Collider>(); // Get the Collider component to ignore during raycasts

        // Check if we should start with the object falling
        if (dropAtStart)
        {
            isFalling = true; // Start falling
        }
    }

    void Update()
    {
        if (isFalling)
        {
            FallToGround();
        }
        else
        {
            HandleMouseInput();
        }
    }

    // Handle mouse input for dragging
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Begin dragging the object if clicking on it
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.transform == transform)
                {
                    isDragging = true;
                    offset = transform.position - hitInfo.point;
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            DragObject();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // Let the object fall until it hits the ground
    void FallToGround()
    {
        // Cast a ray downward to check for the ground
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance, groundLayer))
        {
            // If the object has reached the ground, align it and stop falling
            if (hitInfo.distance <= 0.1f)
            {
                AlignWithSurface(hitInfo);
                isFalling = false;
            }
            else
            {
                // Move the object downward each frame
                transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
            }
        }
        else
        {
            // Keep the object falling if no ground detected
            transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
        }
    }

    // Align the object with the surface
    void AlignWithSurface(RaycastHit hitInfo)
    {
        // Calculate half the object's height
        float objectBottom = objRenderer.bounds.min.y;
        float objectHeight = objRenderer.bounds.size.y;
        float objectHalfHeight = objectHeight / 2;

        Vector3 newPosition = hitInfo.point;
        newPosition.y += objectHalfHeight;
        
        transform.position = newPosition;

        transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
    }

    // Drag the object on the ground or dragtop
    void DragObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance);
        RaycastHit? groundHit = null;
        RaycastHit? dragtopHit = null;

        // Process hits to find ground and dragtop hits
        foreach (RaycastHit hit in hits)
        {
            // Ignore the object's own collider
            if (hit.collider == objCollider)
            {
                continue;
            }

            // Check if hit is on ground layer
            if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
            {
                if (groundHit == null || hit.distance < groundHit.Value.distance)
                {
                    groundHit = hit;
                }
            }
            // Check if hit is on dragtop layer
            else if (((1 << hit.collider.gameObject.layer) & dragtopLayer) != 0)
            {
                if (dragtopHit == null || hit.distance < dragtopHit.Value.distance)
                {
                    dragtopHit = hit;
                }
            }
        }

        if (dragtopHit != null)
        {
            // Update the position based on mouse movement, keeping the object aligned with the dragtop surface
            Vector3 targetPosition = dragtopHit.Value.point + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Align the object to the surface normal
            AlignWithSurface(dragtopHit.Value);
        }
        else if (groundHit != null)
        {
            // Update the position based on mouse movement, keeping the object aligned with the ground surface
            Vector3 targetPosition = groundHit.Value.point + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Align the object to the surface normal
            AlignWithSurface(groundHit.Value);
        }
    }
}

