using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    IDataService dataService = new JsonDataService();
    public PlayerStateJSON currentPlayerState = null;

    public PhoneAudioManager phoneAudioManager;
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
    [SerializeField] private Transform sphere1;
    [SerializeField] private Transform sphere2;
    [SerializeField] private Transform sphere3;
    [SerializeField] private GameObject lossScreenGroup;
    [SerializeField] private TextMeshProUGUI lossText;
    [SerializeField] private float keyRotationSpeed = 90f; // degrees per second
    [SerializeField] private List<GameObject> secCamCheckSpheres;
    [SerializeField] private float JumpForce = 3f;
    [SerializeField] private float GroundCheckDistance = 1.2f;
    [SerializeField] private float jumpCooldown = 0.25f;
    private float lastJumpTime = -999f;

    private bool isGrounded;

    private bool isActive = false;

    public void CheckGrounded()
    {
        int mask = (1 << LayerMask.NameToLayer("SecurityCamCheck") | (1 << LayerMask.NameToLayer("PlayerHidden")));
        int finalMask = ~mask;
        float sphereRadius = 0.3f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hitInfo, GroundCheckDistance, finalMask);

    }

    

    [ServerRpc(RequireOwnership = false)]
    public void RequestClientStateRestore_ServerRPC()
    {
        RestorePlayerState();
    }

    public void RestorePlayerState()
    {
        var phonePlayer = phonePlayerParent.GetComponentInChildren<PhonePlayer>();
        if (phonePlayer)
        {
            phonePlayer.RestorePlayerState_ClientRPC(JsonConvert.SerializeObject(currentPlayerState));
        }
    }

    public void SaveState()
    {
        if (IsServer)
        {
            Debug.Log("try save state");
            var startTime = DateTime.Now.Ticks;
            if (dataService.SaveData("/save.json", currentPlayerState))
            {
                Debug.Log("Data saved successfully in " + (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond + "ms");
            }
            else
            {
                Debug.Log("Failed to save data.");
            }
        }
    }

    public void LoadState()
    {
        if (IsServer)
        {
            Debug.Log("try load state");

            try
            {
                var startTime = DateTime.Now.Ticks;
                var playerData = dataService.LoadData<PlayerStateJSON>("/save.json");

                Debug.Log("player data loaded in " + (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond + "ms");

                if (playerData != null)
                {
                    currentPlayerState = playerData;
                }

                RestorePlayerState();
                CleanupUnreferencedPhotos(currentPlayerState.Photos);

            }
            catch (Exception e)
            {
                Debug.LogError("Error loading data: " + e.Message);
            }
        }
    }

    private void CleanupUnreferencedPhotos(List<PhotoJSON> validPhotos)
    {
        string[] photoFiles = System.IO.Directory.GetFiles(Application.persistentDataPath, "photo_*.png");
        HashSet<string> validPaths = new HashSet<string>(validPhotos.Select(p => p.ImagePath));

        foreach (var file in photoFiles)
        {
            if (!validPaths.Contains(file))
            {
                try
                {
                    System.IO.File.Delete(file);
                    Debug.Log($"🧹 Deleted unused photo file: {file}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to delete {file}: {ex.Message}");
                }
            }
        }
    }


    public void SetLossScreen(string message)
    {
        lossText.text = message;
        lossScreenGroup.SetActive(true);
    }

    public void SetPlayerActive(bool value)
    {
        isActive = value;
        if (value == true)
        {
            lossScreenGroup.SetActive(false);
        }
    }

    public List<Transform> GetSpheres()
    {
        return new List<Transform> { sphere1, sphere2, sphere3 };
    }

    public NetworkVariable<bool> flashlightOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [ServerRpc(RequireOwnership = false)]
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
    }

    public void ResyncPosition()
    {
        if (IsServer)
        {
            transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
        }
    }

    // private void HandleSceneEvent(SceneEvent sceneEvent)
    // {
    //     if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
    //     {
    public void OnLoadCompleted(ulong clientId, string sceneName, LoadSceneMode mode) {


        if (IsServer && clientId == OwnerClientId)
        {
            Debug.Log($"Player load completed: {sceneName} - {IsServer} - {clientId} - {OwnerClientId}");
            var playerSpawn = GameObject.Find("PlayerSpawn");
            if (playerSpawn)
            {
                if (rb == null) rb = GetComponent<Rigidbody>();

                SetPlayerActive(true);

                // Reset velocity to avoid unwanted movement
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;


                // Set position and rotation using Rigidbody
                // rb.position = playerSpawn.transform.position;
                // rb.rotation = playerSpawn.transform.rotation;
                Debug.Log($"Setting position and rotation to {playerSpawn.transform.position} - {playerSpawn.transform.rotation}");
                GetComponent<NetworkTransform>().transform.SetPositionAndRotation(playerSpawn.transform.position, playerSpawn.transform.rotation);

                // If using MovePosition/MoveRotation instead, it must be inside FixedUpdate or coroutine
                // rb.MovePosition(playerSpawn.transform.position);
                // rb.MoveRotation(playerSpawn.transform.rotation);

            }
            else
            {
                Debug.Log("PlayerSpawn not found in scene.");
            }
            
        }

        }
        
    // }


    public override void OnNetworkSpawn()
    {
        // NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        // NetworkManager.Singleton.SceneManager.OnLoadCompleted += OnLoadCompleted;
        if (IsServer && NetworkManager.Singleton.LocalClientId == OwnerClientId)
        {
            LoadState();
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadCompleted;
            flashlightOn.Value = false;
        }

        if (!IsServer)
        {
            foreach (var sphere in secCamCheckSpheres)
            {
                sphere.SetActive(false);
            }
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

        SetPlayerActive(true);
    }

    void Update()
    {
        if (!IsServer) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        if (isActive)
        {
            HandleLook();
        }
    }

    void FixedUpdate() // Physics-based movement should be in FixedUpdate
    {
        if (!IsOwner) return;

        if (isActive)
        {
            CheckGrounded();
            HandleMovement();
            HandleJump();
        }
    }

    void HandleJump()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded && Time.time - lastJumpTime >= jumpCooldown)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
        }
    }


    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;
        rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Optional: Replace mouseX with 0 if you want to disable mouse look
        float keyInput = 0f;
        if (Input.GetKey(KeyCode.Q)) keyInput -= 1f;
        if (Input.GetKey(KeyCode.E)) keyInput += 1f;

        float keyRotation = keyInput * keyRotationSpeed * Time.deltaTime;
        float totalYRotation = mouseX + keyRotation;

        // Pitch (up/down) from mouse only
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        fpsHolder.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Apply yaw (left/right)
        transform.Rotate(Vector3.up * totalYRotation);
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
