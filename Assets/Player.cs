using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public float speed = 5f;
    private CharacterController charController;
    private Camera computerPlayerCamera;

    void Start()
    {
        computerPlayerCamera = transform.Find("ComputerPlayerCamera").GetComponent<Camera>();
        charController = GetComponent<CharacterController>();

        if (!IsServer)
        {
            computerPlayerCamera.enabled = false;
            charController.enabled = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    void Update()
    {
        if (IsServer)
        {
            // transform.position = Position.Value;
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            charController.Move(move * speed * Time.deltaTime);
        }
    }
}