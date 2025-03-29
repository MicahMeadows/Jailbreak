using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public NetworkObject computerPlayerPrefab;
    public NetworkObject phonePlayerPrefab;
    public NetworkObject dronePrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            InstantiatePlayer(computerPlayerPrefab, OwnerClientId);
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        print($"Client {clientId} connected.");
        if (IsServer && clientId != OwnerClientId)
        {
            InstantiatePlayer(phonePlayerPrefab, clientId);
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(dronePrefab, clientId);
        }
    }

    private void InstantiatePlayer(NetworkObject playerPrefab, ulong clientId)
    {
        Debug.Log("Should be spawning player...");
        if (playerPrefab != null)
        {
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab, clientId, false, true);
        }
    }
}
