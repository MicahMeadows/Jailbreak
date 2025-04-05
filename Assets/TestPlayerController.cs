using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    public float lookSpeed = 2f; // Speed of mouse look

    private Camera playerCamera;
    private float pitch = 0f; // Vertical look angle

    // Start is called once before the first frame update
    void Start()
    {
        // Get the player's camera (ensure the player has a camera attached)
        playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        LookAround();
    }

    // Function to handle player movement
    void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down arrow

        // Calculate movement direction based on the player's rotation
        Vector3 moveDirection = (transform.right * horizontal + transform.forward * vertical).normalized;

        // Move the player based on the calculated direction
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    // Function to handle player looking around
    void LookAround()
    {
        // Get mouse movement input for horizontal and vertical movement
        float yaw = Input.GetAxis("Mouse X") * lookSpeed; // Horizontal (left/right) mouse movement
        pitch -= Input.GetAxis("Mouse Y") * lookSpeed; // Vertical (up/down) mouse movement
        pitch = Mathf.Clamp(pitch, -90f, 90f); // Limit the vertical look to avoid flipping upside down

        // Rotate the player horizontally (Yaw) around the Y-axis
        transform.Rotate(Vector3.up * yaw);

        // Rotate the camera vertically (Pitch)
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
