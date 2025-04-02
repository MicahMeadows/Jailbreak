using Unity.Netcode;
using UnityEngine;

public class DroneControl : NetworkBehaviour
{
    enum CurDirection {
        NONE,
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
    }

    enum CurRotation {
        NONE,
        LEFT,
        RIGHT,
    }

    enum CurElevation {
        NONE,
        UP,
        DOWN,
    }

    [SerializeField] private GameObject droneCam;

    private CurRotation curRotation = CurRotation.NONE;
    private CurDirection curDirection = CurDirection.NONE;
    private CurElevation curElevation = CurElevation.NONE;
    private Rigidbody rb;

    private float moveSpeed = 3f;
    private float rotateSpeed = 150f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    public void OnForward()
    {
        curDirection = CurDirection.FORWARD;
    }

    public void OffForward()
    {
        if (curDirection == CurDirection.FORWARD)
        {
            curDirection = CurDirection.NONE;
        }
    }

    public void OnBackward()
    {
        curDirection = CurDirection.BACKWARD;
    }

    public void OffBackward()
    {
        if (curDirection == CurDirection.BACKWARD)
        {
            curDirection = CurDirection.NONE;
        }
    }

    public void OnRight()
    {
        curDirection = CurDirection.RIGHT;
    }

    public void OffRight()
    {
        if (curDirection == CurDirection.RIGHT)
        {
            curDirection = CurDirection.NONE;
        }
    }

    public void OnLeft()
    {
        curDirection = CurDirection.LEFT;
    }

    public void OffLeft()
    {
        if (curDirection == CurDirection.LEFT)
        {
            curDirection = CurDirection.NONE;
        }
    }

    public void OnRotateLeft()
    {
        curRotation = CurRotation.LEFT;
    }

    public void OffRotateLeft()
    {
        if (curRotation == CurRotation.LEFT)
        {
            curRotation = CurRotation.NONE;
        }
    }

    public void OnRotateRight()
    {
        curRotation = CurRotation.RIGHT;
    }

    public void OffRotateRight()
    {
        if (curRotation == CurRotation.RIGHT)
        {
            curRotation = CurRotation.NONE;
        }
    }

    public void OnUp()
    {
        curElevation = CurElevation.UP;
    }

    public void OffUp()
    {
        if (curElevation == CurElevation.UP)
        {
            curElevation = CurElevation.NONE;
        }
    }

    public void OnDown()
    {
        curElevation = CurElevation.DOWN;
    }

    public void OffDown()
    {
        if (curElevation == CurElevation.DOWN)
        {
            curElevation = CurElevation.NONE;
        }
    }


    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        droneCam.SetActive(false);
    }

    public void SetDroneCamActive(bool value)
    {
        droneCam.SetActive(value);
    }

    void FixedUpdate()
    {
        if (IsServer) // Only the client should control the drone
        {
            return;
        }

        Vector3 moveDirection = Vector3.zero;

        // Forward & Backward movement
        if (curDirection == CurDirection.FORWARD)
        {
            moveDirection += transform.forward;
        }
        else if (curDirection == CurDirection.BACKWARD)
        {
            moveDirection -= transform.forward;
        }

        // Left & Right strafing
        if (curDirection == CurDirection.LEFT)
        {
            moveDirection -= transform.right;
        }
        else if (curDirection == CurDirection.RIGHT)
        {
            moveDirection += transform.right;
        }

        // Elevation Up & Down
        if (curElevation == CurElevation.UP)
        {
            moveDirection += transform.up;
        }
        else if (curElevation == CurElevation.DOWN)
        {
            moveDirection -= transform.up;
        }

        // Apply movement
        rb.linearVelocity = moveDirection.normalized * moveSpeed;

        // Apply rotation
        if (curRotation == CurRotation.LEFT)
        {
            rb.angularVelocity = Vector3.up * -rotateSpeed * Mathf.Deg2Rad;
        }
        else if (curRotation == CurRotation.RIGHT)
        {
            rb.angularVelocity = Vector3.up * rotateSpeed * Mathf.Deg2Rad;
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }


    void Update()
    {
        
    }
}
