using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickDropNew : MonoBehaviour
{
    public LayerMask groundLayer; // Use a single ground layer
    public LayerMask dragtopLayer;   // Assign the layer named "dragtop"
    public float raycastDistance = 200f; // The max distance for raycasting
    public float targetRaycastDistance = 8f; // The max distance for raycasting
    public float magnetRadius = 3f;         // Radius to detect nearby dragtop objects
    public float magnetHeightThreshold = 2.5f;
    public float moveSpeed = 10f; // Speed for moving the object while dragging
    public float fallSpeed = 5f; // Speed at which the cube falls to the ground
    public bool dropAtStart = false; // Toggle for whether to drop the cube at the start

    private Camera mainCamera;
    private bool isDragging = false;
    private bool isFalling = false;
    private Vector3 offset;
    private Renderer objRenderer; // The Renderer component to get the object's bounds
    private Collider objCollider; 
    private bool isOnDragtop = false;       // Whether the object is currently on a dragtop object
    private Transform currentDragtop;       // The dragtop object the object is currently on
    public LayerMask Targets;  
    
    public LayerMask mouseRaycastLayerMask;
    private float yOffset = 0.3f; // Y-axis offset for alignment adjustment

    void Start()
    {
        mainCamera = Camera.main;
        objRenderer = GetComponent<Renderer>(); // Get the Renderer component for bounds
        objCollider = GetComponent<Collider>(); // Get the Collider component to ignore during raycasts

        // Check if we should start with the cube falling
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
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //     if (Physics.Raycast(ray, out RaycastHit hitInfo))
        //     {
        //         if (hitInfo.transform == transform)
        //         {
        //             isDragging = true;
        //             offset = transform.position - hitInfo.point;
        //             Debug.Log("Object picked up for dragging");
        //         }
        //         else
        //         {
        //             Debug.Log("Mouse click detected, but not on the object.");
        //         }
        //     }
        // }
        if (Input.GetMouseButtonDown(0)){
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            float sphereCastRadius = .2f;
            if (Physics.SphereCast(ray, sphereCastRadius * 0.75f, out RaycastHit hitInfo, raycastDistance, mouseRaycastLayerMask))
            {
                if (hitInfo.transform == transform)
                {
                    isDragging = true;
                    GameManager.Instance.AnyDragging = true;
                    offset = transform.position - hitInfo.point;
                    Debug.Log("Hit" + hitInfo.collider.gameObject.name);
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
            GameManager.Instance.AnyDragging = false;
            Debug.Log("Object dropped");
            DropObject();
        }
    }


    // Let the cube fall until it hits the ground
    void FallToGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance, groundLayer))
        {
            if (hitInfo.distance <= 0.1f)
            {
                AlignWithSurface(hitInfo);
                isFalling = false;
            }
            else
            {
                transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
            }
        }
        else
        {
            transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
        }
    }

    // Align the cube with the surface
    void AlignWithSurface(RaycastHit hitInfo)
    {
        float objectBottom = objRenderer.bounds.min.y;
        float objectHeight = objRenderer.bounds.size.y;
        float objectHalfHeight = objectHeight / 2;

        Vector3 newPosition = hitInfo.point;
        newPosition.y += objectHalfHeight;
        transform.position = newPosition;

        // transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        Vector3 surfaceNormal = hitInfo.normal;
    
        // Ensure the object only aligns on the XZ plane, ignoring vertical (Y) alignment
        surfaceNormal.y = 0; // Remove Y component to prevent tilting when hitting sides

        // Only rotate the object to align its top with the surface, ignoring the tilt
        if (surfaceNormal != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(surfaceNormal, Vector3.up);
            transform.rotation = targetRotation;
        }
    }

    // Drag the object on the ground
    void DragObject()
    {
          Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

    if (Physics.Raycast(ray, out RaycastHit mouseHit, raycastDistance, ~0, QueryTriggerInteraction.Ignore))
    {
        if (mouseHit.collider.CompareTag("Small")) // Ignore objects tagged as 'small'
        {
            return;
        }

        // Calculate a target position based on the ray hit and offset
        Vector3 targetPosition = mouseHit.point + offset;


        // Magnet detection for dragtop objects
        Collider[] hitColliders = Physics.OverlapSphere(targetPosition, magnetRadius, dragtopLayer, QueryTriggerInteraction.Ignore);
        Collider closestDragtop = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in hitColliders)
        {
            if (collider == objCollider || collider.CompareTag("Small")) // Ignore 'small' tagged objects
                continue;

            float distance = Vector3.Distance(targetPosition, collider.bounds.center);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestDragtop = collider;
            }
        }

        if (closestDragtop != null)
        {
            float verticalDistance = Mathf.Abs(targetPosition.y - closestDragtop.bounds.max.y);
            if (verticalDistance <= magnetHeightThreshold)
            {
                isOnDragtop = true;
                currentDragtop = closestDragtop.transform;

                float objectHalfHeight = objRenderer.bounds.size.y / 2;
                targetPosition.y = closestDragtop.bounds.max.y + objectHalfHeight;

                transform.position = targetPosition;

                // Preserve Y-axis rotation, reset X and Z axes
                Vector3 currentRotation = transform.eulerAngles;
                transform.rotation = Quaternion.Euler(0, currentRotation.y, 0); // Keep Y rotation
                return;
            }
        }

        // If not on dragtop, check if we are near the ground
        if (isOnDragtop)
        {
            Ray downRay = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(downRay, out RaycastHit downHit, raycastDistance, dragtopLayer))
            {
                if (downHit.collider.transform == currentDragtop)
                {
                    float objectHalfHeight = objRenderer.bounds.size.y / 2;
                    targetPosition.y = currentDragtop.GetComponent<Collider>().bounds.max.y + objectHalfHeight;
                    transform.position = targetPosition;

                    Vector3 currentRotation = transform.eulerAngles;
                    transform.rotation = Quaternion.Euler(0, currentRotation.y, 0); // Keep Y rotation
                    return;
                }
                else
                {
                    isOnDragtop = false;
                    currentDragtop = null;
                }
            }
            else
            {
                isOnDragtop = false;
                currentDragtop = null;
            }
        }

        // Check if on ground layer
        Ray groundRay = new Ray(targetPosition + Vector3.up * 10f, Vector3.down);
        if (Physics.Raycast(groundRay, out RaycastHit groundHit, raycastDistance, groundLayer))
        {
            if (groundHit.collider.CompareTag("small")) // Ignore 'small' tagged objects
            {
                return;
            }

            float objectHalfHeight = objRenderer.bounds.size.y / 2;
            targetPosition.y = groundHit.point.y + objectHalfHeight;
            transform.position = targetPosition;

            // Preserve Y-axis rotation, reset X and Z axes
            Vector3 currentRotation = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0); // Keep Y rotation
        }
    }
    }

    void DropObject()
    {
        Vector3[] directions = {
            transform.forward,
            -transform.forward,
            transform.right,
            -transform.right,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized,
            (-transform.forward + transform.right).normalized,
            (-transform.forward - transform.right).normalized
        };

        foreach (Vector3 direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, targetRaycastDistance, Targets))
            {
                if (hit.collider != null && hit.collider.gameObject != this.gameObject)
                {
                    Debug.Log("Target detected, moving to its position");
                    transform.position = hit.transform.position;
                    break;
                }
            }
        }
    }
}
