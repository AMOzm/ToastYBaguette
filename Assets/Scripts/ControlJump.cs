using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlJump : MonoBehaviour
{
    public AnimationClip animationClip;  // Dropdown for selecting animation
    public GameObject[] targetObjects;   // Array of target objects to jump to
    public float travelTime = 2f;        // Time to reach the target object
    public float arcHeight = 2f;         // Height of the jump arc
    public Vector3 offset = Vector3.zero; // Offset for fine-tuning the landing position
    public bool loop = false;            // Should the jumping loop through the array?

    private bool isMoving = false;       // Flag to check if object is moving
    private Vector3 startPosition;       // Start position of the object
    private Vector3 targetPosition;      // Target position (top of the target object)
    private float moveProgress = 0f;     // Tracks movement progress
    private Animation anim;              // Reference to the Animation component
    private Collider fbxCollider;        // Collider reference of the FBX object
    private float fbxHeight;             // Height of the FBX object
    private float targetHeight;          // Height of the target object
    private int currentTargetIndex = 0;  // Index of the current target in the array

    void Start()
    {
        // Get the Animation component from the FBX object
        anim = GetComponent<Animation>();

        // Ensure the GameObject has an Animation component
        if (anim == null)
        {
            Debug.LogError("No Animation component found on this GameObject.");
        }
        else
        {
            // Add the selected animation clip to the Animation component
            anim.AddClip(animationClip, animationClip.name);
        }

        // Ensure the target object array is assigned and contains items
        if (targetObjects == null || targetObjects.Length == 0)
        {
            Debug.LogError("Target object array is not assigned or is empty.");
            return;
        }

        // Use a simpler collider like BoxCollider or SphereCollider for click detection
        fbxCollider = GetComponent<Collider>();
        if (fbxCollider == null)
        {
            Debug.LogError("Collider for click detection is not found on the FBX object.");
            return;
        }

        // Calculate the height of this FBX object using collider bounds
        fbxHeight = fbxCollider.bounds.size.y;
    }

    void OnMouseDown()
    {
        if (!isMoving && targetObjects != null && targetObjects.Length > 0 && animationClip != null)
        {
            // Get the current target object
            GameObject currentTargetObject = targetObjects[currentTargetIndex];

            // Ensure the current target object has a collider for height calculation
            Collider targetCollider = currentTargetObject.GetComponent<Collider>();
            if (targetCollider == null)
            {
                Debug.LogError("Target object is missing a collider.");
                return;
            }

            // Calculate the height of the current target object using collider bounds
            targetHeight = targetCollider.bounds.size.y;

            // Set the start position and calculate the precise target position
            startPosition = transform.position;

            // Calculate the Y position at the top of the current target object using collider bounds
            targetPosition = new Vector3(
                currentTargetObject.transform.position.x,
                currentTargetObject.transform.position.y + (targetHeight / 2) + (fbxHeight / 2),
                currentTargetObject.transform.position.z
            ) + offset; // Apply the offset to fine-tune landing position

            // Play the animation
            anim.Play(animationClip.name);

            // Adjust the animation speed to match the travel time
            anim[animationClip.name].speed = animationClip.length / travelTime;

            // Start moving the object
            isMoving = true;
            moveProgress = 0f;
        }
    }

    void Update()
    {
        if (isMoving)
        {
            // Move the object from the start position to the target position over time
            moveProgress += Time.deltaTime / travelTime;

            // Horizontal movement (X and Z) using Lerp
            Vector3 currentHorizontalPosition = Vector3.Lerp(startPosition, targetPosition, moveProgress);

            // Add a parabolic arc to the Y-axis
            float arc = Mathf.Sin(moveProgress * Mathf.PI) * arcHeight;

            // Set the current position with the arc applied to the Y-axis
            transform.position = new Vector3(
                currentHorizontalPosition.x,
                Mathf.Lerp(startPosition.y, targetPosition.y, moveProgress) + arc, // Adding the arc to the Y position
                currentHorizontalPosition.z
            );

            // If movement is complete, stop moving and update target index
            if (moveProgress >= 1f)
            {
                isMoving = false;
                UpdateTargetIndex();
            }
        }
    }

    // Updates the target index for the next jump
    void UpdateTargetIndex()
    {
        if (currentTargetIndex < targetObjects.Length - 1)
        {
            currentTargetIndex++;
        }
        else
        {
            if (loop)
            {
                currentTargetIndex = 0;  // Loop back to the first target
            }
            else
            {
                currentTargetIndex = targetObjects.Length - 1;  // Stop at the last target
            }
        }
    }
}
