using System.Collections;
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
        var currentState = computerPlayer.GetPlayerState();

        if (currentState.LevelState.Intro == false)
        {
            HandleIntro();
            return;
        }
    }

    private void HandleIntro()
    {
        StartCoroutine(TextIntroSoon());
    }

    private IEnumerator TextIntroSoon()
    {
        yield return new WaitForSeconds(.1f);
        Debug.Log("Sending text message to phone player.");
        phonePlayer.SendIncomingText("Yo dude!", "Friend");
    }
}
