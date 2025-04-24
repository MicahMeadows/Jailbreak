using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HomeBaseLevelManager : NetworkBehaviour
{
    private Player computerPlayer;
    private PhonePlayer phonePlayer;

    public override void OnNetworkSpawn()
    {

        if (IsServer)
        {
            computerPlayer = GameObject.FindGameObjectWithTag("ComputerPlayer").GetComponent<Player>();
            phonePlayer = GameObject.FindGameObjectWithTag("PhonePlayer").GetComponent<PhonePlayer>();

            InitializeLevel();
        }
    }

    public override void OnDestroy()
    {
        phonePlayer.OffTextReceived(OnTextReceived);
        phonePlayer.OffBubbleTapped(OnBubbleTapped);
        base.OnDestroy();
    }

    private void InitializeLevel()
    {
        phonePlayer.OnTextReceived(OnTextReceived);
        phonePlayer.OnBubbleTapped(OnBubbleTapped);
    }

    void OnBubbleTapped(string messageId)
    {
        Debug.Log("Bubble tapped: " + messageId);
        if (messageId == "intro-p2")
        {
            computerPlayer.SaveState();
            phonePlayer.ChangeLevel_ServerRPC("GasStation");
        }
    }

    void OnTextReceived(NetworkTextMessage message)
    {
        if (message.Contact == "Friend")
        {
            switch (message.Message)
            {
                case "intro-p1-r1":
                case "intro-p1-r2":
                    var awaitable = IntroPart2();
                    break;
            }
        }
    }

    private async Awaitable IntroPart2()
    {
        await Awaitable.WaitForSecondsAsync(4f);
        phonePlayer.SendIncomingText("intro-p2", "Friend", (msg) => {
            Debug.Log("Reply handler called: " + msg);
        });
    }

    public async Awaitable ExitDoorTriggerEntered()
    {
        if (!IsServer) return;

        if (computerPlayer.currentPlayerState.LevelState.Intro == false)
        {
            await TextIntro();
            return;
        }
    }

    private async Awaitable TextIntro()
    {
        computerPlayer.currentPlayerState.LevelState.Intro = true;
        await Awaitable.WaitForSecondsAsync(5f);
        phonePlayer.SendIncomingText("intro-p1", "Friend", async (msg) => {
            Debug.Log("Reply handler called: " + msg);
            await Awaitable.WaitForSecondsAsync(2f);
            Debug.Log("Test delay called...");
        });
    }

    // private IEnumerator TextIntroSoon()
    // {
    //     computerPlayer.currentPlayerState.LevelState.Intro = true;
    //     yield return new WaitForSeconds(5f);
    //     phonePlayer.SendIncomingText("intro-p1", "Friend");
    // }
}
