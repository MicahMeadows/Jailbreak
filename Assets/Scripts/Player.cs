using System;
using System.Collections.Generic;
using System.Data;
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
    PlayerStateJSON currentPlayerState = new PlayerStateJSON();

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

    private bool isActive = false;


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
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading data: " + e.Message);
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
            HandleMovement();
        }
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
