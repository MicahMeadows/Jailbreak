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

    private NetworkObject computerPlayer;
    private NetworkObject phonePlayer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            Debug.Log($"Should spawn at: {playerSpawnPoint.position}");
            // computerPlayer = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(computerPlayerPrefab, clientId, false, false, false, spawnPos, playerSpawnPoint.rotation);

            computerPlayer = Instantiate(computerPlayerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            computerPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(OwnerClientId);

            phonePlayer = InstantiatePlayer(phonePlayerPrefab, 999999);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        print($"Client {clientId} connected.");
        if (IsServer)
        {
            if (clientId != OwnerClientId)
            {
                if (phonePlayer == null)
                {
                    phonePlayer = InstantiatePlayer(phonePlayerPrefab, clientId);
                }
                else
                {
                    phonePlayer.ChangeOwnership(clientId);
                }
                // NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(dronePrefab, clientId, false, false, false, droneSpawnPoint.position);
            }
        }
    }

    private NetworkObject InstantiatePlayer(NetworkObject playerPrefab, ulong clientId)
    {
        if (playerPrefab != null)
        {
            Debug.Log("Spawn point is null, spawning at default location.");
            return NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab, clientId, false, true);
        }
        return null;
    }
}
