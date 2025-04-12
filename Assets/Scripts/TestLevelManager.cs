using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLevelManager : NetworkBehaviour
{
    public NetworkObject computerPlayer;
    public NetworkObject phonePlayer;

    private List<SecurityCamera> securityCameras = new List<SecurityCamera>();
    private List<FoodShelf> foodShelves = new List<FoodShelf>();
    private List<ExitDoor> exitDoors = new List<ExitDoor>();

    void FailGame()
    {
        Debug.Log("You were seen stealing food. you lose!");
        if (IsServer)
        {
            var player = computerPlayer.GetComponent<Player>();
            if (player)
            {
                player.SetLossScreen("You were seen stealing food by a camera...");
                player.SetPlayerActive(false);
            }
            Invoke("GoBackToHomeBase", 3f);
        }
    }

    private void GoBackToHomeBase()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("HomeBase", LoadSceneMode.Single);
    }

    void OnExitDoor()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("HomeBase", LoadSceneMode.Single);
        }
    }

    void OnItemStolen()
    {
        foreach (var cam in securityCameras)
        {
            if (cam.IsPlayerDetected())
            {
                FailGame();
                return;
            }
        }

        TestPhoneCall();

        foreach (var exit in exitDoors)
        {
            exit.SetCanExit(true);
        }
    }

    void TestPhoneCall()
    {
        phonePlayer.GetComponent<PhonePlayer>().CreateIncomingCall("Test Caller", "hello");
    }

    public override void OnNetworkSpawn()
    {
        computerPlayer = GameObject.FindGameObjectWithTag("ComputerPlayer").GetComponent<NetworkObject>();
        phonePlayer = GameObject.FindGameObjectWithTag("PhonePlayer").GetComponent<NetworkObject>();

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
