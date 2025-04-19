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
        // ...
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
        phonePlayer.SendIncomingText("intro-hello", "Friend");
    }
}
