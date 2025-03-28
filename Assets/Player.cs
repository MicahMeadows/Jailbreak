using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public float speed = 5f;
    private CharacterController charController;
    [SerializeField] private GameObject computerPlayerCam;
    private GameObject canvas;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        canvas = GetComponentInChildren<Canvas>().gameObject;
        charController = GetComponent<CharacterController>();

        if (!IsServer)
        {
            canvas.SetActive(false);
            computerPlayerCam.SetActive(false);
            charController.enabled = false;
        }
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