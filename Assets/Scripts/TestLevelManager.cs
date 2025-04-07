using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TestLevelManager : NetworkBehaviour
{
    public NetworkObject computerPlayerPrefab;
    public NetworkObject phonePlayerPrefab;

    private List<SecurityCamera> securityCameras = new List<SecurityCamera>();
    private List<FoodShelf> foodShelves = new List<FoodShelf>();

    void FailGame()
    {
        Debug.Log("You were seen stealing food. you lose!");
    }

    void OnItemStolen()
    {
        foreach (var cam in securityCameras)
        {
            if (cam.IsPlayerDetected())
            {
                FailGame();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        computerPlayerPrefab = GameObject.FindGameObjectWithTag("ComputerPlayer").GetComponent<NetworkObject>();
        phonePlayerPrefab = GameObject.FindGameObjectWithTag("PhonePlayer").GetComponent<NetworkObject>();

        securityCameras = FindObjectsByType<SecurityCamera>(FindObjectsSortMode.None).ToList();
        foodShelves = FindObjectsByType<FoodShelf>(FindObjectsSortMode.None).ToList();

        foreach (var shelf in foodShelves)
        {
            shelf.onStolen.AddListener(OnItemStolen);
        }
    }
}
