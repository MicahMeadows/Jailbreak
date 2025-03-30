using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public float speed = 5f;
    public float mouseSensitivity = 100f;
    private CharacterController charController;
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
        base.OnNetworkSpawn();

        flashlightOn.OnValueChanged += OnFlashlightValueChanged;

        canvas = GetComponentInChildren<Canvas>().gameObject;
        charController = GetComponent<CharacterController>();

        if (!IsOwner)
        {
            canvas.SetActive(false);
            computerPlayerCam.SetActive(false);
            charController.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (IsServer)
        {
            playerMesh.enabled = false;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleLook();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        charController.Move(move * speed * Time.deltaTime);
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
