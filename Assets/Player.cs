using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public bool IsComputerPlayer = false;
    public bool IsLocal = false;

    void Start()
    {
        var tags = CurrentPlayer.ReadOnlyTags();
        bool isThisComputer = tags.Contains("ComputerPlayer") && IsOwner;
        bool isRemoteComputer = tags.Contains("PhonePlayer") && !IsOwner;
        IsComputerPlayer = isThisComputer || isRemoteComputer;

        if (isThisComputer) 
        {
            gameObject.name = "ComputerPlayerLocal";
        } 
        else if (isRemoteComputer) 
        {
            gameObject.name = "ComputerPlayerNetworked";
        } 
        else if (IsOwner) 
        {
            gameObject.name = "PhonePlayerLocal";
        }
        else 
        {
            gameObject.name = "PhonePlayerNetwork";
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
        transform.position = Position.Value;
    }
}