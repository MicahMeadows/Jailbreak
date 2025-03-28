using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    private bool IsComputerPlayer = false;
    public float speed = 5f;
    private CharacterController charController;
    private Camera computerPlayerCamera;

    void Start()
    {
        computerPlayerCamera = transform.Find("ComputerPlayerCamera").GetComponent<Camera>();
        charController = GetComponent<CharacterController>();
        var tags = CurrentPlayer.ReadOnlyTags();
        bool isThisComputer = tags.Contains("ComputerPlayer") && IsOwner;
        bool isRemoteComputer = tags.Contains("PhonePlayer") && !IsOwner;
        IsComputerPlayer = isThisComputer || isRemoteComputer;

        if (IsComputerPlayer)

        if (isThisComputer) 
        {
            gameObject.name = "ComputerPlayerLocal";
        } 
        else if (isRemoteComputer) 
        {
            gameObject.name = "ComputerPlayerNetworked";
        } 
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Move();
        }
        Debug.Log($"IsOwner: {IsOwner}");
    }

    public void Move()
    {
        SubmitPositionRequestRpc();
    }

    [Rpc(SendTo.Server)]
    void SubmitPositionRequestRpc(RpcParams rpcParams = default)
    {
        var randomPosition = GetRandomPositionOnPlane();
        transform.position = randomPosition;
        Position.Value = randomPosition;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    void Update()
    {
        if (IsComputerPlayer && IsOwner)
        {
            // transform.position = Position.Value;
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            charController.Move(move * speed * Time.deltaTime);
        }
    }
}