using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSphere : MonoBehaviour
{
    public Transform sphereCenterA; // The center of the sphere
    public Transform sphereCenterB; // The other sphere center
    public float sphereRadius = 10f; // The radius of the sphere
    public float rotationSpeed = 50f; // Speed of camera movement along the sphere surface
    public float zoomSpeed = 2f; // Speed at which to zoom in
    public float zoomOutSpeed = 2f;
    public float maxZoomRadius = 5f; // Minimum radius when zoomed in

    private float originalRadius; // To store the original sphere radius
    private float theta; // Horizontal angle (rotation around the Y-axis)
    private float phi;   // Vertical angle (rotation around the X-axis)
    private Transform currentSphereCenter; // The currently active sphere center

    void Start()
    {
        GameObject centerOneObject = GameObject.FindWithTag("C1");
        if (centerOneObject != null)
        {
            sphereCenterA = centerOneObject.transform;
        }

        GameObject centerTwoObject = GameObject.FindWithTag("C2");
        if (centerTwoObject != null)
        {
            sphereCenterB = centerTwoObject.transform;
        }

        // Initialize the angles (you can set them to any initial values)
        theta = -45f;
        phi = 15f;

        // Start with sphereCenterA as the active center
        currentSphereCenter = sphereCenterA;

        // Store the original radius
        originalRadius = sphereRadius;
    }

    void Update()
    {
        // Zoom in while space is pressed
        if (Input.GetKey(KeyCode.Space))
        {
            sphereRadius = Mathf.Lerp(sphereRadius, maxZoomRadius, zoomSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            sphereRadius = Mathf.Lerp(sphereRadius, originalRadius, zoomOutSpeed * Time.deltaTime);
        }

        // Rotate the camera based on the current sphere center
        CameraRotate(currentSphereCenter);
    }

    private void SwitchSphereCenter()
    {
        // Switch the current sphere center between A and B
        currentSphereCenter = (currentSphereCenter == sphereCenterA) ? sphereCenterB : sphereCenterA;
    }

    private void CameraRotate(Transform sphereCenter)
    {
        // Get input from WASD keys
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D keys for left/right movement
        float verticalInput = Input.GetAxis("Vertical");     // W/S keys for up/down movement

        // Update angles based on WASD input
        theta -= horizontalInput * rotationSpeed * Time.deltaTime; // Left/Right movement (A/D)
        phi += verticalInput * rotationSpeed * Time.deltaTime;     // Up/Down movement (W/S)

        // Clamp phi so the camera doesn't flip upside down
        phi = Mathf.Clamp(phi, -89f, 89f);

        // Convert spherical coordinates to Cartesian coordinates
        float x = sphereRadius * Mathf.Cos(Mathf.Deg2Rad * phi) * Mathf.Sin(Mathf.Deg2Rad * theta);
        float y = sphereRadius * Mathf.Sin(Mathf.Deg2Rad * phi);
        float z = sphereRadius * Mathf.Cos(Mathf.Deg2Rad * phi) * Mathf.Cos(Mathf.Deg2Rad * theta);

        // Set camera position relative to the sphere center
        Vector3 cameraPosition = new Vector3(x, y, z);
        transform.position = sphereCenter.position + cameraPosition;

        // Make the camera always look at the center of the sphere
        transform.LookAt(sphereCenter.position);
    }
}
