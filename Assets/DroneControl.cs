using Unity.Netcode;
using UnityEngine;

public class DroneControl : NetworkBehaviour
{
    enum CurDirection {
        NONE,
        FORWARD,
        BACKWARD,
    }

    enum CurRotation {
        NONE,
        LEFT,
        RIGHT,
    }

    [SerializeField] private GameObject droneCam;

    private CurRotation curRotation = CurRotation.NONE;
    private CurDirection curDirection = CurDirection.NONE;
    private Rigidbody rb;

    private float moveSpeed = 5f;
    private float rotateSpeed = 100f;


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

    public void OnLeft()
    {
        curRotation = CurRotation.LEFT;
    }

    public void OffLeft()
    {
        if (curRotation == CurRotation.LEFT)
        {
            curRotation = CurRotation.NONE;
        }
    }

    public void OnRight()
    {
        curRotation = CurRotation.RIGHT;
    }

    public void OffRight()
    {
        if (curRotation == CurRotation.RIGHT)
        {
            curRotation = CurRotation.NONE;
        }
    }


    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if (!IsServer) // Is Phone Client
        {
            droneCam.SetActive(true);
        }
        else // Is Computer Client
        {
            droneCam.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (IsServer) // The client should control the drone
        {
            return;
        }

        Vector3 moveDirection = Vector3.zero;

        if (curDirection == CurDirection.FORWARD)
        {
            moveDirection = transform.forward;
        }
        else if (curDirection == CurDirection.BACKWARD)
        {
            moveDirection = -transform.forward;
        }

        if (moveDirection != Vector3.zero)
        {
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero; // Stop when no input
        }

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
            rb.angularVelocity = Vector3.zero; // Stop rotation when no input
        }
    }

    void Update()
    {
        
    }
}
