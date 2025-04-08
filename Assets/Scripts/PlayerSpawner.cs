using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    public NetworkObject computerPlayerPrefab;
    public NetworkObject phonePlayerPrefab;
    public NetworkObject dronePrefab;
    public Transform droneSpawnPoint;
    public Transform playerSpawnPoint;
    public string initialScene;

    private NetworkObject computerPlayer;
    private NetworkObject phonePlayer;
    private NetworkObject drone;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            computerPlayer = InstantiatePlayer(computerPlayerPrefab, OwnerClientId, playerSpawnPoint.position, playerSpawnPoint.rotation);

            phonePlayer = InstantiatePlayer(phonePlayerPrefab, 999999);

            // drone = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(dronePrefab, 999999, false, false, false, droneSpawnPoint.position);

            NetworkManager.SceneManager.LoadScene(initialScene, LoadSceneMode.Single);
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
                if (drone)
                {
                    drone.ChangeOwnership(clientId);
                }
            }
        }
    }

    private NetworkObject InstantiatePlayer(NetworkObject playerPrefab, ulong clientId, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
    {
        if (playerPrefab != null)
        {
            Debug.Log("Spawn point is null, spawning at default location.");
            var newPlayer = Instantiate(playerPrefab, position, rotation);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            return newPlayer;
        }
        return null;
    }
}
