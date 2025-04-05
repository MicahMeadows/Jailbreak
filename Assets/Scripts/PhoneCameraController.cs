using UnityEngine;

public class PhoneCameraController : MonoBehaviour
{
    [SerializeField] private GameObject phoneCamera; // Use Transform for local rotations
    public float rotationSpeed = 0.2f;
    private Vector3 lastPanPosition;
    private bool isPanning;
    private bool isActive = false;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float minVerticalAngle = -60f; // Clamp for looking up/down
    private float maxVerticalAngle = 60f;

    public void SetEnabled(bool value)
    {
        isActive = value;
        phoneCamera.gameObject.SetActive(isActive);
        if (value == false)
        {
            xRotation = 0f;
            yRotation = 0f;
            phoneCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }

    void Update()
    {
        if (!isActive) return;

        Vector2 delta = Vector2.zero;

        // Handle touch input (Mobile)
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                lastPanPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                delta = touch.position - (Vector2)lastPanPosition;
                lastPanPosition = touch.position;
            }
        }
        // Handle mouse input (PC)
        else if (Input.GetMouseButtonDown(0))
        {
            isPanning = true;
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && isPanning)
        {
            delta = (Vector2)(Input.mousePosition - lastPanPosition);
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
        }

        if (delta != Vector2.zero)
        {
            // Adjust rotations
            yRotation += -delta.x * rotationSpeed; // Horizontal rotation
            xRotation += delta.y * rotationSpeed;  // Vertical rotation

            // Clamp vertical rotation
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

            // Apply local rotation
            phoneCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }
}
