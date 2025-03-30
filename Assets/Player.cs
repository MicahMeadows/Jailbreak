using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 100f;
    private Rigidbody rb; // Replacing CharacterController with Rigidbody
    [SerializeField] private GameObject computerPlayerCam;
    [SerializeField] private GameObject fpsHolder;
    private GameObject canvas;
    [SerializeField] private MeshRenderer playerMesh;
    private float xRotation = 0f;
    [SerializeField] private GameObject flashlight;

    public NetworkVariable<bool> flashlightOn = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [ServerRpc(RequireOwnership=false)]
    public void ToggleFlashlight_ServerRPC(ServerRpcParams rpcParams = default)
    {
        flashlightOn.Value = !flashlightOn.Value;
    }

    private void OnFlashlightValueChanged(bool prev, bool cur)
    {
        flashlight.SetActive(cur);
    }
    
    public override void OnNetworkSpawn()
    {
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

        if (IsServer)
        {
            playerMesh.enabled = false;
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

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        fpsHolder.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
