using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolWObstacles : MonoBehaviour
{
    [System.Serializable]
    public class PatrolPoint
    {
        public Transform point;      // The position of the patrol point
        public bool isJumpPoint;     // Mark if this is a jump point
        public float customJumpHeight = -1; // Custom jump height for this point (-1 for default)
        public float customTurnSpeed = -1;  // Custom turn speed for this point (-1 for default)

        // Constructor to initialize default values
        public PatrolPoint()
        {
            customJumpHeight = -1;
            customTurnSpeed = -1;
        }
    }

    public PatrolPoint[] patrolPoints;   // Array of patrol points
    public float speed = 2f;             // Movement speed, default is 2
    public float jumpDuration = 1.5f;    // Duration of the jump (time to complete jump)
    public float arcHeight = 2f;         // Global default for the maximum height of the jump
    public float turnSpeed = 5f;         // Global default for turn speed
    public float radius = 0.5f;          // Radius to check distance to the next patrol point
    public bool loop = true;             // Toggle to loop through patrol points, default is true
    public bool allowTilting = true;     // Toggle to allow tilting while jumping, default is true
    public bool stationaryPoint = true;  // Toggle to decide whether to jump to the stationary target point (true by default)
    public bool chaseJumpTarget = false; // Toggle to chase the target after landing if it has moved (false by default)
    public bool useRaycast = false;      // Toggle to enable raycast detection for obstacles
    public bool obstacleBehind = true;   // Toggle to stop for obstacles even behind the target

    public bool slowdown = true;         // New Toggle for slowdown or stop behavior
    public float slowSpeed = 2f;         // Time in seconds to slow down when encountering an obstacle
    public float raycastRange = 5f;      // Range of the raycast for detecting obstacles
    public LayerMask obstacleLayer;      // Layer to check for obstacles
    [SerializeField] private float currentSpeed;          // Variable to adjust speed based on raycast detection

    private int currentPointIndex = 0;   // Index of the current patrol point
    private Transform targetPoint;       // The current target patrol point
    private bool isJumping = false;      // Check if currently jumping
    private Vector3 startPos;            // Start position for the jump
    private float jumpProgress = 0f;     // Progress of the jump (0 to 1)
    private float jumpStartTime;         // The time the jump started
    private Quaternion initialRotation;  // Store initial rotation for non-tilting jumps
    private Vector3 precalculatedTargetPosition; // Precalculated position for non-stationary points
    public float rotationSpeed = 4f;
    public GameObject inactiveChild; // Child to deactivate when speed changes
    public GameObject activeChild;   // Child to activate after 1 second
    public GameObject prefabToSpawn; // Prefab to spawn
    public GameObject OprefabToSpawn; //Other Prefab to Spawn
    public GameObject JprefabToSpawn; //Other Prefab to Spawn
    public Transform prefabSpawnLocation; // Optional location for prefab spawn
    public Transform prefabSpawnNewLocation;
    private bool isPrefabSpawned = false; // Track if prefab is spawned
    private bool OPrefabSpawned = false;
    private bool JPrefabSpawned = false;
    [SerializeField] private bool isSlowing = false;
    private bool changing = false;
    [SerializeField] private GameObject PawPrintPrefab;
    [SerializeField] private Transform spawnLocation1;
    [SerializeField] private Transform spawnLocation2;
    [SerializeField] private float spawnInterval = 0.5f; // Spawn interval in seconds
    private bool spawnAtFirstLocation = true;
    private bool PawPActive = false;
    [SerializeField] private int PawOffset;
    public GameObject spherePrefab; // Assign your sphere prefab in the inspector
    public Vector2 sizeRange = new Vector2(6f, 20f); // Min and max random sizes
    public float upwardForce = 2f; // Force applied upwards
    public float lifetime = 1f; // Time before the spheres are destroyed
    public int numberOfSpheres = 20; // Number of spheres to spawn
    public int framesBetweenSpawns = 1; // Number of frames to wait between spawns

    void Start()
    {
        if (patrolPoints.Length > 0)
        {
            SetTargetPoint(patrolPoints[currentPointIndex]); // Set initial target
        }

        currentSpeed = speed; // Initialize currentSpeed
        UpdateChildStates();
    }

    void Update()
    {
        PatrolMovement();
        UpdateChildStates();
        if ((currentSpeed <= 0.11f && currentSpeed >= 0.09f) && isSlowing){
            Debug.Log("Here");
            activeChild.SetActive(false);
            inactiveChild.SetActive(false);
            HandleSpeedChange(false);
        }
        if((currentSpeed <= 0.11f && currentSpeed >= 0.09f) && !isSlowing){
            activeChild.SetActive(false);
            inactiveChild.SetActive(false);
            HandleSpeedChange(true);
        }
    }
    void PatrolMovement(){
        if (targetPoint == null) return; // Exit if no target point is set

        // Check if the current point is a jump point
        PatrolPoint currentPatrolPoint = patrolPoints[currentPointIndex];

        if (currentPatrolPoint.isJumpPoint)
        {
            if (!isJumping)
            {
                // Before starting the jump, check if an obstacle is too high
                if (CanJumpOverObstacle(currentPatrolPoint))
                {
                    StartJump(currentPatrolPoint);
                }
                else
                {
                    Debug.Log("Obstacle is too high. Can't jump.");
                    GetNextPatrolPoint(); // If the obstacle is too high, move to the next patrol point without jumping
                }
            }
            else
            {
                PerformJump(currentPatrolPoint);
            }
        }
        else
        {
            MoveTowardsPoint(currentPatrolPoint);
        }
    }
    void UpdateChildStates()
    {
        // When speed is positive, activeChild should be active and inactiveChild should be inactive
        if (currentSpeed >= 0.1f && !changing)
        {
            if(PawPActive == false){
                StartCoroutine(SpawnAtAlternatingLocations());
                PawPActive = true;
            }
            if (activeChild != null)
            {
                activeChild.SetActive(true);
            }

            if (inactiveChild != null)
            {
                inactiveChild.SetActive(false);
            }
        }
        // When speed is 0, activeChild should be inactive and inactiveChild should be active
        else if (currentSpeed < 0.1f && !changing)
        {
            if(PawPActive == true){
                StopCoroutine(SpawnAtAlternatingLocations());
                PawPActive = false;
            }
            if (activeChild != null)
            {
                activeChild.SetActive(false);
            }

            if (inactiveChild != null)
            {
                inactiveChild.SetActive(true);
            }
        }
    }
    private IEnumerator SpawnAtAlternatingLocations()
    {
        while (true) // Infinite loop to keep spawning
        {
            // Determine the spawn location and toggle for the next spawn
            Transform spawnLocation = spawnAtFirstLocation ? spawnLocation1 : spawnLocation2;
            spawnAtFirstLocation = !spawnAtFirstLocation;
            // Set the rotation with 180 degrees on the Y-axis
            Quaternion spawnRotation = spawnLocation.rotation * Quaternion.Euler(0, PawOffset, 0);
            // Instantiate the prefab at the chosen location with the calculated rotation
            Instantiate(PawPrintPrefab, spawnLocation.position, spawnRotation);
            // Wait for the specified interval before spawning the next object
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Function to determine if the character can jump over the obstacle
    bool CanJumpOverObstacle(PatrolPoint patrolPoint)
    {
        Vector3 direction = GetTargetDirection();
        float distanceToTarget = Vector3.Distance(transform.position, GetTargetPosition());

        // Cast a ray to detect obstacles between the character and the target
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction.normalized, out hit, distanceToTarget, obstacleLayer))
        {
            // Use the collider bounds to get the top of the obstacle
            Collider obstacleCollider = hit.collider;
            float obstacleTop = obstacleCollider.bounds.max.y;

            // Calculate the maximum height of the jump arc based on the distance to the target
            float maxJumpHeight = transform.position.y + arcHeight;  // Max jump height is the starting Y + arcHeight

            // Compare the top of the obstacle with the maximum jump height
            if (obstacleTop <= maxJumpHeight)
            {
                // If the obstacle top is less than or equal to the jump arc, we can jump over it
                return true;
            }
            else
            {
                // If the obstacle is too tall, we can't jump over it
                return false;
            }
        }

        // If no obstacle is detected, we can proceed with the jump
        return true;
    }

    void MoveTowardsPoint(PatrolPoint patrolPoint)
    {
        if (useRaycast)
        {
            // Perform raycast to check for obstacles in the direction of movement
            RaycastHit hit;
            Vector3 direction = GetTargetDirection();
            float distanceToTarget = Vector3.Distance(transform.position, GetTargetPosition());

            // Perform raycast in the direction of movement, limited by raycastRange
            if (Physics.Raycast(transform.position, direction.normalized, out hit, raycastRange, obstacleLayer))
            {
                float distanceToObstacle = hit.distance;
                Debug.Log("Hit object: " + hit.collider.gameObject.name);

                if (obstacleBehind)
                {
                    // If ObstacleBehind = true, stop for any obstacle within range
                    AdjustSpeed(0f);  // Gradually slow down or stop based on Slowdown toggle
                }
                else
                {
                    // If ObstacleBehind = false, stop only if the obstacle is closer than the target
                    if (distanceToObstacle < distanceToTarget)
                    {
                        //HandleSpeedChange(false);
                        AdjustSpeed(0f);  // Gradually slow down or stop based on Slowdown toggle
                        isSlowing = true;
                         Debug.Log("Obstacle in front. Stopping.");
                    }
                    else
                    {
                        isSlowing = false;
                        AdjustSpeed(speed);  // Gradually restore speed
                    }
                }
            }
            else
            {
                isSlowing = false;
                //HandleSpeedChange(true);
                // If no obstacle, restore speed and move
                AdjustSpeed(speed);  // Gradually restore speed
            }
        }
        else
        {
            currentSpeed = speed; // If raycast is not enabled, use normal speed
        }

        Vector3 moveDirection = GetTargetDirection();
        transform.Translate(moveDirection.normalized * currentSpeed * Time.deltaTime, Space.World);

        // Determine turn speed (use custom if available, otherwise use global)
        float currentTurnSpeed = patrolPoint.customTurnSpeed != -1 ? patrolPoint.customTurnSpeed : turnSpeed;

        // Rotate towards the target point
        Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * currentTurnSpeed);
        RotateTowardsTarget();

        // Check if the object is within the radius of the target point
        if (Vector3.Distance(transform.position, GetTargetPosition()) <= radius)
        {
            GetNextPatrolPoint(); // Move to the next patrol point
        }
    }
    void HandleSpeedChange(bool toWalk)
    {
        // Detect when speed changes from 0 to positive
        if (toWalk && !isPrefabSpawned)
        {
            //SpawnSmokePuff();
            // Deactivate the specified child
            UpdateChildStates();

            // Spawn the prefab as a child
            Vector3 spawnPositionWithOffset = new Vector3(prefabSpawnLocation.position.x, prefabSpawnLocation.position.y - 0f, prefabSpawnLocation.position.z);
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPositionWithOffset, transform.rotation, transform);

            isPrefabSpawned = true;

            // Destroy the prefab after 1 second and activate the other child
            StartCoroutine(HandlePrefabLifecycle(spawnedPrefab, true));
            changing = true;
        }

        // Detect when speed changes from positive to 0
        else if (!toWalk && !OPrefabSpawned)
        {
            //SpawnSmokePuff();
            // Deactivate the active child
            UpdateChildStates();

            // Spawn another prefab when speed changes to 0
            Vector3 spawnPositionWithOffset = new Vector3(prefabSpawnLocation.position.x, prefabSpawnLocation.position.y - 0f, prefabSpawnLocation.position.z);
            GameObject spawnedPrefab = Instantiate(OprefabToSpawn, spawnPositionWithOffset, transform.rotation, transform);
            OPrefabSpawned = true;
            
            // Destroy the prefab after 1 second and reactivate the inactive child
            StartCoroutine(HandlePrefabLifecycle(spawnedPrefab, false));
            changing = true;
        }
    }
    IEnumerator HandlePrefabLifecycle(GameObject spawnedPrefab, bool activatingActiveChild)
    {
        SpawnSmokePuff();
        activeChild.SetActive(false);
        inactiveChild.SetActive(false);
        // Wait for 1 second
        yield return new WaitForSeconds(1.5f);

        // Destroy the prefab
        Destroy(spawnedPrefab);
        changing = false;

        if (activatingActiveChild)
        {
            // Activate the other child
            if (activeChild != null)
            {
                activeChild.SetActive(true);
                isPrefabSpawned = false; // Reset for future speed changes
            }
        }
        else
        {
            // Reactivate the inactive child when speed goes back to 0
            if (inactiveChild != null)
            {
                inactiveChild.SetActive(true);
                OPrefabSpawned = false; // Reset for future speed changes
            }
        }
    }
        
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

    // Adjust the speed when encountering an obstacle or restoring movement
    void AdjustSpeed(float targetSpeed)
    {
        if (slowdown)
        {
            // Gradually slow down based on the specified slowSpeed time
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / slowSpeed);
        }
        else
        {
            // Stop immediately if Slowdown is false
            currentSpeed = targetSpeed;
        }
    }

    void StartJump(PatrolPoint patrolPoint)
    {
        startPos = transform.position;   // Save the starting position
        jumpStartTime = Time.time;       // Save the time when the jump started
        initialRotation = transform.rotation;  // Store the initial rotation for non-tilting
        isJumping = true;                // Indicate that the object is jumping
        jumpProgress = 0f;               // Reset jump progress

        // Rotate towards the target point BEFORE jumping starts
        RotateTowardsTarget(patrolPoint);
    }

    void PerformJump(PatrolPoint patrolPoint)
    {
        // Determine jump height (use custom if available, otherwise use global)
        float currentJumpHeight = patrolPoint.customJumpHeight != -1 ? patrolPoint.customJumpHeight : arcHeight;

        // Calculate the normalized progress of the jump (from 0 to 1)
        jumpProgress = (Time.time - jumpStartTime) / jumpDuration;

        // Linear interpolation between the start position and the target point (horizontal movement)
        Vector3 horizontalPos = Vector3.Lerp(startPos, GetTargetPosition(), jumpProgress);

        // Calculate the vertical position using a parabolic equation
        float heightOffset = currentJumpHeight * Mathf.Sin(Mathf.PI * jumpProgress);  // Sinusoidal arc for smooth jump
        Vector3 newPos = new Vector3(horizontalPos.x, horizontalPos.y + heightOffset, horizontalPos.z);

        // Rotate towards the target point WHILE jumping
        RotateTowardsTarget(patrolPoint);
        
        if(JPrefabSpawned == false){
        Vector3 spawnPositionWithOffset = new Vector3(prefabSpawnNewLocation.position.x, prefabSpawnNewLocation.position.y - 0f, prefabSpawnNewLocation.position.z);
            GameObject spawnedPrefab = Instantiate(JprefabToSpawn, spawnPositionWithOffset, transform.rotation, transform);
            JPrefabSpawned = true;
            
            // Destroy the prefab after 1 second and reactivate the active child
            StartCoroutine(HandlePrefabLifecycle(spawnedPrefab, true));
            changing = true;
        }
        // Update the object's position
        transform.position = newPos;

        // If the jump is complete
        if (jumpProgress >= 1f)
        {
            isJumping = false;  // Stop jumping
            JPrefabSpawned = false;

            // Reset the character's rotation if tilting is allowed
            if (allowTilting)
            {
                transform.rotation = initialRotation;
            }

            // If ChaseJumpTarget is enabled and StationaryPoint is disabled, check if the target has moved and jump again
            if (!stationaryPoint && chaseJumpTarget)
            {
                if (Vector3.Distance(transform.position, targetPoint.position) > radius)
                {
                    // Set new target to current position of the patrol point (target moved)
                    SetTargetPoint(patrolPoints[currentPointIndex]);

                    // Start a new jump towards the new target position
                    StartJump(patrolPoint);
                    return;
                }
            }

            GetNextPatrolPoint();  // Move to the next patrol point
        }
    }

    // New function to rotate towards the target point
    void RotateTowardsTarget(PatrolPoint patrolPoint)
    {
        Vector3 direction = GetTargetDirection();
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Determine turn speed (use custom if available, otherwise use global)
        float currentTurnSpeed = patrolPoint.customTurnSpeed != -1 ? patrolPoint.customTurnSpeed : turnSpeed;

        if (allowTilting)
        {
            // If tilting is allowed, rotate fully (yaw, pitch, and roll)
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * currentTurnSpeed);
        }
        else
        {
            // If tilting is not allowed, only rotate around the Y-axis (yaw)
            Vector3 currentEuler = transform.rotation.eulerAngles;
            Vector3 targetEuler = lookRotation.eulerAngles;
            Quaternion noTiltRotation = Quaternion.Euler(currentEuler.x, targetEuler.y, currentEuler.z);  // Keep pitch and roll
            transform.rotation = Quaternion.Slerp(transform.rotation, noTiltRotation, Time.deltaTime * currentTurnSpeed);
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
        SetTargetPoint(patrolPoints[currentPointIndex]);
    }

    // Function to set the target point and precalculate position if necessary
    void SetTargetPoint(PatrolPoint patrolPoint)
    {
        targetPoint = patrolPoint.point;
        if (!stationaryPoint)
        {
            precalculatedTargetPosition = targetPoint.position; // Store the position of the point when reached
        }
    }

    // Get the target position (either stationary or precalculated)
    Vector3 GetTargetPosition()
    {
        return stationaryPoint ? targetPoint.position : precalculatedTargetPosition;
    }

    // Get the target direction for movement or jumping
    Vector3 GetTargetDirection()
    {
        return GetTargetPosition() - transform.position;
    }
    public void SpawnSmokePuff()
    {
        StartCoroutine(SpawnSpheresOverTime());
    }

    private IEnumerator SpawnSpheresOverTime()
    {
        for (int i = 0; i < numberOfSpheres; i++)
        {
            // Instantiate sphere prefab at the spawn point
            GameObject sphere = Instantiate(spherePrefab, prefabSpawnLocation.position, Quaternion.identity);

            // Set random scale for the sphere
            float randomSize = Random.Range(sizeRange.x, sizeRange.y);
            sphere.transform.localScale = Vector3.one * randomSize;

            // Add a Rigidbody to the sphere if it doesn't already have one
            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = sphere.AddComponent<Rigidbody>();
            }

            // Apply upward force to the sphere
            Vector3 randomDirection = Vector3.up + new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
            rb.AddForce(randomDirection.normalized * upwardForce, ForceMode.Impulse);

            // Destroy the sphere after the specified lifetime
            Destroy(sphere, lifetime);

            // Wait for the specified number of frames before spawning the next sphere
            for (int frame = 0; frame < framesBetweenSpawns; frame++)
            {
                yield return null;
            }
        }
    }
}
