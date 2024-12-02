using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolAdvance : MonoBehaviour
{
    // Start is called before the first frame update
    [System.Serializable]
    public class PatrolPoint
    {
        public Transform point;  // The position of the patrol point
        public bool isJumpPoint; // Mark if this is a jump point
    }

    public PatrolPoint[] patrolPoints;   // Array of patrol points
    public float speed = 2f;             // Movement speed, default is 2
    public float rotationSpeed = 4f;
    public float jumpDuration = 1.5f;    // Duration of the jump (time to complete jump)
    public float arcHeight = 2f;         // The maximum height of the jump
    public float radius = 0.5f;          // Radius to check distance to the next patrol point
    public bool loop = true;             // Toggle to loop through patrol points, default is true
    public bool allowTilting = true;     // Toggle to allow tilting while jumping, default is true
    public float obstacleDetectionDistance = 1.0f; // Distance to detect obstacles
    public LayerMask obstacleLayer;      // Layer mask to specify which layers are considered obstacles

    private int currentPointIndex = 0;   // Index of the current patrol point
    private Transform targetPoint;       // The current target patrol point
    private bool isJumping = false;      // Check if currently jumping
    private Vector3 startPos;            // Start position for the jump
    private float jumpProgress = 0f;     // Progress of the jump (0 to 1)
    private float jumpStartTime;         // The time the jump started
    private Quaternion initialRotation;  // Store initial rotation for non-tilting jumps

    void Start()
    {
        if (patrolPoints.Length > 0)
        {
            targetPoint = patrolPoints[currentPointIndex].point; // Set initial target
        }
    }

    void Update()
    {
        if (targetPoint == null) return; // Exit if no target point is set

        // Check if the current point is a jump point
        PatrolPoint currentPatrolPoint = patrolPoints[currentPointIndex];

        if (currentPatrolPoint.isJumpPoint)
        {
            if (!isJumping)
            {
                StartJump();
            }
            else
            {
                PerformJump();
            }
        }
        else
        {
            MoveTowardsPoint();
        }
    }

    void MoveTowardsPoint()
    {
        Vector3 direction = targetPoint.position - transform.position;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        // // Rotate towards the target point
        // Quaternion lookRotation = Quaternion.LookRotation(direction);
        // transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);

        // // Check if the object is within the radius of the target point
        // if (Vector3.Distance(transform.position, targetPoint.position) <= radius)
        // {
        //     GetNextPatrolPoint(); // Move to the next patrol point
        // }
        // Check for obstacles in the direction of movement
        // if (IsObstacleInPath(direction))
        // {
        //     AvoidObstacle();
        // }
        // else
        // {
        //     transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
        // }
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
        RotateTowardsTarget(); // Rotate to face the target point

        // Check if the object is within the radius of the target point
        if (Vector3.Distance(transform.position, targetPoint.position) <= radius)
        {
            GetNextPatrolPoint(); // Move to the next patrol point
        }
    }

    // bool IsObstacleInPath(Vector3 direction)
    // {
    //     RaycastHit hit;
    //     if (Physics.Raycast(transform.position, direction, out hit, obstacleDetectionDistance, obstacleLayer))
    //     {
    //         if (hit.collider != null && hit.collider.gameObject != this.gameObject)
    //         {
    //             Debug.Log("obstacle observed");
    //             return true; // Obstacle detected
    //         }
    //     }
    //     return false; // No obstacle detected
    // }

    // void AvoidObstacle()
    // {
    //     // Adjust direction slightly to avoid the obstacle
    //     Vector3 avoidDirection = transform.right + transform.forward;
    //     transform.Translate(avoidDirection.normalized * speed * Time.deltaTime, Space.World);

    //     RotateTowardsTarget();
    // }
     void RotateTowardsTarget()
    {
        if (targetPoint == null) return;

        Vector3 directionToTarget = targetPoint.position - transform.position;
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void StartJump()
    {
        startPos = transform.position;   // Save the starting position
        jumpStartTime = Time.time;       // Save the time when the jump started
        initialRotation = transform.rotation;  // Store the initial rotation for non-tilting
        isJumping = true;                // Indicate that the object is jumping
        jumpProgress = 0f;               // Reset jump progress
    }

    void PerformJump()
    {
        // Calculate the normalized progress of the jump (from 0 to 1)
        jumpProgress = (Time.time - jumpStartTime) / jumpDuration;

        // Linear interpolation between the start position and the target point (horizontal movement)
        Vector3 horizontalPos = Vector3.Lerp(startPos, targetPoint.position, jumpProgress);

        // Calculate the vertical position using a parabolic equation
        float heightOffset = arcHeight * Mathf.Sin(Mathf.PI * jumpProgress);  // Sinusoidal arc for smooth jump
        Vector3 newPos = new Vector3(horizontalPos.x, horizontalPos.y + heightOffset, horizontalPos.z);

        // Update the object's position
        transform.position = newPos;

        // Handle rotation (tilting or not tilting)
        if (allowTilting)
        {
            // Rotate towards the target point
            Vector3 direction = targetPoint.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);
        }
        else
        {
            // Keep the original rotation during the jump
            transform.rotation = initialRotation;
        }

        // If the jump is complete
        if (jumpProgress >= 1f)
        {
            isJumping = false;  // Stop jumping
            GetNextPatrolPoint();  // Move to the next patrol point
        }
    }

    void GetNextPatrolPoint()
    {
        // Increment the patrol point index
        currentPointIndex++;

        // Check if the patrol has reached the last point in the array
        if (currentPointIndex >= patrolPoints.Length)
        {
            if (loop)
            {
                currentPointIndex = 0; // Loop back to the first point if looping is enabled
            }
            else
            {
                currentPointIndex--;  // Ensure index stays valid, prevent going out of bounds
                targetPoint = null;   // Stop further movement when no loop is enabled
                return;               // Stop further execution
            }
        }

        // Set the next patrol point
        targetPoint = patrolPoints[currentPointIndex].point;
    }
}
