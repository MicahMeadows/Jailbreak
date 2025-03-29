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
    private CharacterController charController;

    private CurRotation curRotation = CurRotation.NONE;
    private CurDirection curDirection = CurDirection.NONE;

    private float moveSpeed = 5f;
    private float rotateSpeed = 100f;


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
        charController = GetComponent<CharacterController>();
        if (!IsServer) // Is Phone Client
        {
            droneCam.SetActive(true);
        }
        else // Is Computer Client
        {
            droneCam.SetActive(false);
            charController.enabled = false;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        if (IsServer) // Should control drone from client
        {
            return;
        }

        Debug.Log($"Cur Dir: {curDirection}, Cur Rot: {curRotation}");

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
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        if (curRotation == CurRotation.LEFT)
        {
            transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime);
        }
        else if (curRotation == CurRotation.RIGHT)
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}
