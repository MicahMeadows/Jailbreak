using System.Collections;
using TMPro;
using Unity.Netcode;
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

    private void InitializeLevel()
    {
        phonePlayer.OnTextReceived(OnTextReceived);
        phonePlayer.OnBubbleTapped(OnBubbleTapped);
    }

    void OnBubbleTapped(string messageId)
    {
        Debug.Log("Bubble tapped: " + messageId);
    }

    void OnTextReceived(NetworkTextMessage message)
    {
        if (message.Contact == "Friend")
        {
            switch (message.Message)
            {
                case "intro-p1-r1":
                case "intro-p1-r2":
                    StartCoroutine(IntroPart2Soon());
                    break;
            }
        }
    }

    private IEnumerator IntroPart2Soon()
    {
        yield return new WaitForSeconds(4f);
        phonePlayer.SendIncomingText("intro-p2", "Friend");
    }

    public void ExitDoorTriggerEntered()
    {
        if (!IsServer) return;

        if (computerPlayer.currentPlayerState.LevelState.Intro == false)
        {
            StartCoroutine(TextIntroSoon());
            return;
        }
    }

    private IEnumerator TextIntroSoon()
    {
        computerPlayer.currentPlayerState.LevelState.Intro = true;
        yield return new WaitForSeconds(5f);
        phonePlayer.SendIncomingText("intro-p1", "Friend");
    }
}
