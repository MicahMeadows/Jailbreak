using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public NetworkObject computerPlayerPrefab;
    public NetworkObject phonePlayerPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Debug.Log($"On network spawn. IsOwner: {IsOwner}");
        var tags = CurrentPlayer.ReadOnlyTags();
        bool isThisComputer = tags.Contains("ComputerPlayer") && IsOwner;
        bool isRemoteComputer = tags.Contains("PhonePlayer") && !IsOwner;

        if (isThisComputer || isRemoteComputer)
        {
            InstantiatePlayer(computerPlayerPrefab);
        }
        else
        {
            InstantiatePlayer(phonePlayerPrefab);
        }
    }

    private void InstantiatePlayer(NetworkObject playerPrefab)
    {
        Debug.Log("Should be spawning player...");
        if (playerPrefab != null)
        {
            // var instance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            // instance.Spawn();
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab, OwnerClientId, false, true);
        }
    }
}
