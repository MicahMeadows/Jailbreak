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
        }
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
        phonePlayer.SendIncomingText("intro-p1", "Friend", onReply: async (msg) => {
            await Awaitable.WaitForSecondsAsync(4f);
            phonePlayer.SendIncomingText("intro-p2", "Friend", onTap: () => {
                computerPlayer.SaveState();
                phonePlayer.ChangeLevel_ServerRPC("GasStation");
            });
            Debug.Log("done!");
        });
    }
    
}
