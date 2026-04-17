using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float panSpeed = 20f;      // How fast camera moves when dragging
    public float zoomSpeed = 5f;      // How fast camera zooms
    
    [Header("Zoom Limits")]
    public float minZoom = 5f;        // Closest zoom (zoomed in)
    public float maxZoom = 20f;       // Furthest zoom (zoomed out)
    
    [Header("Drag Settings")]
    public bool invertDrag = false;    // Reverse drag direction if needed
    
    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging = false;
    
    void Start()
    {
        cam = GetComponent<Camera>();
    }
    
    void Update()
    {
        HandlePan();
        HandleZoom();
    }
    
    void HandlePan()
    {
        // Check for right mouse button (desktop) OR two-finger touch (mobile)
        bool panTrigger = Input.GetMouseButtonDown(1) || (Input.touchCount == 2 && Input.GetTouch(1).phase == TouchPhase.Began);
        bool panHolding = Input.GetMouseButton(1) || Input.touchCount == 2;
        bool panRelease = Input.GetMouseButtonUp(1) || (Input.touchCount == 0);
        
        // Start dragging
        if (panTrigger)
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
            return;
        }
        
        // End dragging
        if (panRelease)
        {
            isDragging = false;
            return;
        }
        
        // While dragging
        if (isDragging && panHolding)
        {
            Vector3 currentPos = Input.mousePosition;
            Vector3 difference = dragOrigin - currentPos;
            
            // Apply drag direction
            float direction = invertDrag ? -1f : 1f;
            Vector3 move = new Vector3(difference.x, difference.y, 0) * direction * panSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
            
            dragOrigin = currentPos;
        }
    }
    
    void HandleZoom()
    {
        float zoomAmount = 0f;
        
        // Mouse scroll wheel (desktop)
        zoomAmount = Input.GetAxis("Mouse ScrollWheel");
        
        // Pinch zoom (mobile)
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            Vector2 prevPos2 = touch2.position - touch2.deltaPosition;
            
            float prevDistance = Vector2.Distance(prevPos1, prevPos2);
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            
            zoomAmount = (prevDistance - currentDistance) * 0.01f;
        }
        
        if (zoomAmount != 0)
        {
            if (cam.orthographic)
            {
                cam.orthographicSize -= zoomAmount * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }
            else
            {
                // For perspective camera (if you switch later)
                cam.fieldOfView -= zoomAmount * zoomSpeed;
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 30f, 100f);
            }
        }
    }
}