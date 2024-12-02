using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsFall : MonoBehaviour
{
    // Exposed properties
    public float gravity = 9.8f; // Gravity, adjustable in the inspector
    public LayerMask landingLayer; // First layer for landing objects
    public LayerMask secondaryLandingLayer; // Second layer for landing objects
    public float fallSpeed = 0f; // Current fall speed of the object
    public float raycastOffset = 0.1f; // Offset to prevent the object from sticking to the ground
    public float heightAdjustmentOffset = 0f; // Fine-tuning offset for specific objects

    private bool isGrounded = false; // Flag to check if the object is grounded
    private float objectHeight; // Height of the object (from center to bottom)

    void Start()
    {
        // Calculate the height of the object based on its bounds
        objectHeight = GetComponent<Renderer>().bounds.extents.y;
    }

    void Update()
    {
        if (!isGrounded)
        {
            // Apply gravity and make the object fall
            fallSpeed += gravity * Time.deltaTime;
            transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
        }

        // Combine both layers into a single mask for raycasting
        LayerMask combinedLayers = landingLayer;

        if (secondaryLandingLayer != LayerMask.NameToLayer("None")) // Check if secondary layer is set
        {
            combinedLayers |= secondaryLandingLayer; // Combine the layers
        }

        // Raycast downward from the center of the object, considering the height of the object
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, objectHeight + raycastOffset + heightAdjustmentOffset, combinedLayers))
        {
            // Check if the object is close enough to land
            float distanceToGround = hit.distance;

            if (distanceToGround <= objectHeight + raycastOffset + heightAdjustmentOffset)
            {
                // Snap to the surface, adjusted for the height of the object
                transform.position = new Vector3(transform.position.x, hit.point.y + objectHeight + heightAdjustmentOffset, transform.position.z);
                fallSpeed = 0f;
                isGrounded = true;
            }
        }
        else
        {
            // If no ground is detected, the object keeps falling
            isGrounded = false;
        }
    }

    // For debugging, show the raycast in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * (objectHeight + 100f));
    }
}
