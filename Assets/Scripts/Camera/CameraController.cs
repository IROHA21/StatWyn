using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Zoom variables
    public float zoomSpeed = 5f;
    public float minZoomDistance = 5f;
    public float maxZoomDistance = 15f;
    private float currentZoomDistance;

    // Pan variables  
    public float panSpeed = 10f;
    private Vector3 currentTargetPosition;
    
    // Mouse drag pan variables
    private Vector3 dragOrigin;
    private bool isDragging = false;
    public float dragSensitivity = 1f;

    //how up in the air the camera is
    public Vector3 camOffset = new Vector3(0, 10, 0);

    // the target
    public Transform target;

    void Start()
    {
        target = GameObject.Find("Map").transform;
        currentZoomDistance = camOffset.magnitude;
        currentTargetPosition = target.position;
    }

    void Update()
    {
        // Check for mouse button down (left click)
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragOrigin = Input.mousePosition;
        }
        
        // Check for mouse button up
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void HandleMouseDrag()
{
    if (isDragging)
    {
        // Get current mouse position
        Vector3 currentMousePos = Input.mousePosition;
        
        // Calculate difference since last frame
        Vector3 difference = currentMousePos - dragOrigin;
        
        // Move the target position based on mouse movement (now natural direction)
        Vector3 panMovement = new Vector3(
            difference.x * dragSensitivity * Time.deltaTime,
            0,
            difference.y * dragSensitivity * Time.deltaTime
        );
        
        currentTargetPosition += panMovement;
        
        // Update drag origin for smooth continuous dragging
        dragOrigin = currentMousePos;
    }
}

    void LateUpdate()
    {
        // Handle mouse drag panning
        HandleMouseDrag();
        
        // ZOOM: Mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoomDistance -= scroll * zoomSpeed;
        currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);
        
        // Apply position with zoom and pan
        Vector3 zoomedOffset = camOffset.normalized * currentZoomDistance;
        this.transform.position = currentTargetPosition + zoomedOffset;
        this.transform.LookAt(currentTargetPosition);
        
        // Add 180 degrees to Y rotation to correct the reversed view
        this.transform.rotation = Quaternion.Euler(
            this.transform.eulerAngles.x,
            this.transform.eulerAngles.y + 180,
            this.transform.eulerAngles.z
        );
    }
}