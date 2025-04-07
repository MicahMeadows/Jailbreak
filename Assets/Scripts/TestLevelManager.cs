using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLevelManager : NetworkBehaviour
{
    public NetworkObject computerPlayerPrefab;
    public NetworkObject phonePlayerPrefab;

    private List<SecurityCamera> securityCameras = new List<SecurityCamera>();
    private List<FoodShelf> foodShelves = new List<FoodShelf>();
    private List<ExitDoor> exitDoors = new List<ExitDoor>();

    void FailGame()
    {
        Debug.Log("You were seen stealing food. you lose!");
    }

    void OnExitDoor()
    {
        if (IsServer)
        {
            NetworkManager.SceneManager.LoadScene("HomeBase", LoadSceneMode.Single);
        }
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
        foreach (var exit in exitDoors)
        {
            exit.SetCanExit(true);
        }
    }

    public override void OnNetworkSpawn()
    {
        computerPlayerPrefab = GameObject.FindGameObjectWithTag("ComputerPlayer").GetComponent<NetworkObject>();
        phonePlayerPrefab = GameObject.FindGameObjectWithTag("PhonePlayer").GetComponent<NetworkObject>();

        securityCameras = FindObjectsByType<SecurityCamera>(FindObjectsSortMode.None).ToList();
        foodShelves = FindObjectsByType<FoodShelf>(FindObjectsSortMode.None).ToList();
        exitDoors = FindObjectsByType<ExitDoor>(FindObjectsSortMode.None).ToList();

        foreach (var exit in exitDoors)
        {
            exit.onExit.AddListener(OnExitDoor);
        }

        foreach (var shelf in foodShelves)
        {
            shelf.onStolen.AddListener(OnItemStolen);
        }
    }
}
