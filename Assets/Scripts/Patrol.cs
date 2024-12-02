using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    public Transform[] patrolPoints; // Array of patrol points
    public float radius = 0.5f;      // Radius to check distance to the next patrol point
    public float speed = 2f;         // Movement speed, default is 2
    public bool loop = true;         // Toggle to loop through patrol points, default is true

    private int currentPointIndex = 0; // Index of the current patrol point
    private Transform targetPoint;     // The current target patrol point

    void Start()
    {
        if (patrolPoints.Length > 0)
        {
            targetPoint = patrolPoints[currentPointIndex]; // Set initial target
        }
    }

    void Update()
    {
        if (targetPoint == null) return; // Exit if no patrol points are set

        // Move towards the target point
        Vector3 direction = targetPoint.position - transform.position;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        // Rotate towards the target point
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);

        // Check if the patrol object is within the radius of the target point
        if (Vector3.Distance(transform.position, targetPoint.position) <= radius)
        {
            GetNextPatrolPoint(); // Move to the next patrol point
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
                return; // Stop moving if looping is disabled
            }
        }

        // Set the next patrol point
        targetPoint = patrolPoints[currentPointIndex];
    }
}
