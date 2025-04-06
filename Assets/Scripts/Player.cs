using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public GameObject phonePlayerParent;
    public float speed = 5f;
    public float mouseSensitivity = 100f;
    private Rigidbody rb; // Replacing CharacterController with Rigidbody
    [SerializeField] private GameObject computerPlayerCam;
    [SerializeField] private GameObject fpsHolder;
    private GameObject canvas;
    [SerializeField] private MeshRenderer playerMesh;
    private float xRotation = 0f;
    [SerializeField] private GameObject flashlight;

    public NetworkVariable<bool> flashlightOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [ServerRpc(RequireOwnership=false)]
    public void ToggleFlashlight_ServerRPC(ServerRpcParams rpcParams = default)
    {
        Debug.Log($"Toggle RPC Hit on server. Should go to: {!flashlightOn.Value}");
        flashlightOn.Value = !flashlightOn.Value;
        Debug.Log($"Value after swap: {flashlightOn.Value}");
    }

    public void OnHitByLaser()
    {
        Debug.Log("hit by laser!");
    }

    private void OnFlashlightValueChanged(bool prev, bool cur)
    {
        flashlight.SetActive(cur);
    }

    void Start()
    {
        NetworkManager.SceneManager.OnSceneEvent += HandleSceneEvent;
    }

    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            Debug.Log("Loaded into scene.");
            if (IsServer)
            {
                var playerSpawn = GameObject.Find("PlayerSpawn");
                if (playerSpawn)
                {
                    // TODO: set player pos!
                    transform.position = playerSpawn.transform.position;
                    transform.rotation = playerSpawn.transform.rotation;
                }
                else
                {
                    Debug.Log("PlayerSpawn not found in scene.");
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            flashlightOn.Value = false;
        }

        flashlightOn.OnValueChanged += OnFlashlightValueChanged;

        canvas = GetComponentInChildren<Canvas>().gameObject;
        rb = GetComponent<Rigidbody>();  // Get the Rigidbody component

        if (!IsOwner)
        {
            canvas.SetActive(false);
            computerPlayerCam.SetActive(false);
            rb.isKinematic = true; // Prevents physics simulation on non-owner players
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            rb.freezeRotation = true; // Prevents unwanted rotation
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleLook();
    }

    void FixedUpdate() // Physics-based movement should be in FixedUpdate
    {
        if (!IsOwner) return;

        HandleMovement();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;
        rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Calculate new X rotation (clamping the pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply the X rotation to the FPS holder (camera)
        fpsHolder.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player (y-axis)
        // Instead of directly rotating, we will accumulate rotation in a variable
        float targetRotationY = transform.eulerAngles.y + mouseX; // add rotation based on input
        targetRotationY = NormalizeAngle(targetRotationY); // Ensure rotation stays within 0-360 degrees range

        // Apply the calculated Y rotation to the player's body (ignoring pitch for body)
        transform.rotation = Quaternion.Euler(0f, targetRotationY, 0f);
    }

    // Normalize angle to the range 0-360 to avoid unexpected rotation behavior
    float NormalizeAngle(float angle)
    {
        if (angle < 0f)
            angle += 360f;
        else if (angle >= 360f)
            angle -= 360f;
        
        return angle;
    }
}
