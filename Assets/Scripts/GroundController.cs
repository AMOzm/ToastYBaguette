using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]private float yOffset = 0.3f; // Y-axis offset for alignment adjustment
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.AnyDragging) // Only allow adjustments while dragging
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                transform.position += new Vector3(0, yOffset, 0); // Move object up
                Debug.Log("Moved object up");
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                transform.position -= new Vector3(0, yOffset, 0); // Move object down
                Debug.Log("Moved object down");
            }
        }
    }
}
