using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickDropItems : MonoBehaviour
{
    public LayerMask groundLayer; // Assign the layer for the plane or hill
    public float raycastDistance = 100f; // The max distance for raycasting
    public float moveSpeed = 10f; // Speed for moving the object while dragging
    public float fallSpeed = 5f; // Speed at which the cube falls to the ground
    public bool dropAtStart = false; // Toggle for whether to drop the cube at the start
    public float tartgetDetectDis = 1.0f;
    public LayerMask TargetPos; 

    private Camera mainCamera;
    private bool isDragging = false;
    private bool isFalling = false;
    private Vector3 offset;

    void Start()
    {
        mainCamera = Camera.main;

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
            //DropObject();
        }
    }

    // Let the cube fall until it hits the ground
    void FallToGround()
    {
        // Cast a ray downward to check for the ground
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance, groundLayer))
        {
            // If the cube has reached the ground, align it and stop falling
            if (hitInfo.distance <= 0.1f)
            {
                AlignWithSurface(hitInfo);
                isFalling = false;
            }
            else
            {
                // Move the cube downward each frame
                transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
            }
        }
        else
        {
            // Keep the cube falling if no ground detected
            transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
        }
    }

    // Align the cube with the surface
    void AlignWithSurface(RaycastHit hitInfo)
    {
        // Move the cube to the hit point
        transform.position = hitInfo.point + hitInfo.normal * (transform.localScale.y / 12f);

        // Rotate the cube to align with the surface normal
        transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
    }

    // Drag the object on the ground
    void DragObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance, groundLayer))
        {
            // Update the position based on mouse movement, keeping the cube aligned with the surface
            Vector3 targetPosition = hitInfo.point + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Align the cube to the surface normal
            AlignWithSurface(hitInfo);
        }
    }
    // bool DropItem(Vector3 direction)
    // {
    //     // RaycastHit hit;
    //     // if (Physics.Raycast(transform.position, direction, out hit, tartgetDetectDis, TargetPos))
    //     // {
    //     //     if (hit.collider != null && hit.collider.gameObject != this.gameObject)
    //     //     {
    //     //         Debug.Log("target in range");
    //     //         return true; // Obstacle detected
    //     //     }
    //     // }
    //     // return false; // No obstacle detected
    // }
}
