using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLevelManager : NetworkBehaviour
{
    public NetworkObject computerPlayer;
    public NetworkObject phonePlayer;

    private List<SecurityCamera> securityCameras = new List<SecurityCamera>();
    private List<FoodShelf> foodShelves = new List<FoodShelf>();
    private List<ExitDoor> exitDoors = new List<ExitDoor>();

    void FailLevel()
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
            computerPlayer.GetComponent<Player>().LoadState();
            Invoke("GoBackToHomeBase", 3f);
        }
    }

    void GoBackToHomeBase()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("HomeBase", LoadSceneMode.Single);
    }

    void WinLevel()
    {
        computerPlayer.GetComponent<Player>().SaveState();
        GoBackToHomeBase();
    }

    void OnExitDoor()
    {
        WinLevel();
    }

    void OnItemStolen()
    {
        foreach (var cam in securityCameras)
        {
            if (cam.IsPlayerDetected())
            {
                FailLevel();
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
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();
        if (phonePlayerController)
        {
            phonePlayerController.CreateIncomingCall("Test Caller", () => {
                phonePlayerController.phoneAudioManager.PlayAudio("hello", onComplete: () => {
                    phonePlayerController.HangupCall();
                });
            });
        }
        
    }

    async Awaitable SendFrickerText(string contact)
    {
        await Awaitable.WaitForSecondsAsync(2f);
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();
        phonePlayerController.SendIncomingText("send-cube-2", contact);

        await Awaitable.WaitForSecondsAsync(2f);

        phonePlayerController.CreateIncomingCall(contact, () => {
            _ = PlaySexyCube();
        });
    }

    private async Awaitable PlaySexyCube()
    {
        await Awaitable.WaitForSecondsAsync(1f);
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();

        phonePlayerController.phoneAudioManager.PlayAudio("sexy-cube", onComplete: () => {
            _ = Hangup();
        });
    }

    // private IEnumerator HangupSoon()
    private async Awaitable Hangup()
    {
        // yield return new WaitForSeconds(1f);
        await Awaitable.WaitForSecondsAsync(1f);
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();
        phonePlayerController.HangupCall();
        _ = TextHero();
    }

    // private IEnumerator HeroSoon()
    private async Awaitable TextHero()
    {
        // yield return new WaitForSeconds(1f);
        await Awaitable.WaitForSecondsAsync(1f);
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();
        phonePlayerController.SendIncomingText("send-cube-3", "Cube Lover");
    }

    public override void OnDestroy()
    {
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();
        phonePlayerController.OffTextReceived(OnTextReceived);
    }

    void OnTextReceived(NetworkTextMessage message)
    {
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();
        if (message.Contact == "Cube Lover")
        {
            if (message.ImageObjects != null)
            {
                foreach (var imageObject in message.ImageObjects)
                {
                    Debug.Log("img obj: " + imageObject);
                    if (imageObject == "GreenCube")
                    {
                        _ = SendFrickerText(message.Contact);
                    }
                }
            }
        }
        else
        {
            phonePlayerController.SendIncomingText("?", message.Contact);
        }
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

        if (IsServer)
        {
            _ = StartLevel();
        }
    }

    private async Awaitable StartLevel()
    {
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();
        phonePlayerController.OnTextReceived(OnTextReceived);
        await TextPlayerSoon();
    }

    async Awaitable TextPlayerSoon()
    {
        await Awaitable.WaitForSecondsAsync(2f);
        var phonePlayerController = phonePlayer.GetComponent<PhonePlayer>();
        phonePlayerController.SendIncomingText("send-cube", "Cube Lover");
    }
}
