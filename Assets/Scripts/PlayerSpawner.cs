using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public NetworkObject computerPlayerPrefab;
    public NetworkObject phonePlayerPrefab;
    public NetworkObject dronePrefab;
    public Transform droneSpawnPoint;
    public Transform playerSpawnPoint;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        print($"Client {clientId} connected.");
        if (IsServer)
        {
            if (clientId == OwnerClientId)
            {
                // InstantiatePlayer(computerPlayerPrefab, OwnerClientId, playerSpawnPoint);
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(computerPlayerPrefab, clientId, false, false, false, playerSpawnPoint.position, playerSpawnPoint.rotation);
            }
            else
            {
                InstantiatePlayer(phonePlayerPrefab, clientId);
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(dronePrefab, clientId, false, false, false, droneSpawnPoint.position);
            }
        }
    }

    private void InstantiatePlayer(NetworkObject playerPrefab, ulong clientId)
    {
        Debug.Log("Should be spawning player...");
        if (playerPrefab != null)
        {
            Debug.Log("Spawn point is null, spawning at default location.");
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab, clientId, false, true);
        }
    }
}
