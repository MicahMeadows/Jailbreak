using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public struct DroneControllerAppState
{
    public bool isActive;
}

public struct PhoneState
{
    public DroneControllerAppState droneControllerAppState;
}


public class PhonePlayer : NetworkBehaviour
{
    private GameObject computerPlayer = null;
    private GameObject cube;
    private GameObject canvas;

    private PhoneState phoneState;

    public void ToggleFlashlight()
    {
        if (!IsServer)
        {
            computerPlayer.GetComponent<Player>().ToggleFlashlight_ServerRPC();
        }
    }

    public override void OnNetworkSpawn()
    {
        cube = transform.Find("Cube").gameObject;
        canvas = GetComponentInChildren<Canvas>().gameObject;
        base.OnNetworkSpawn();
        if (IsServer)
        {
            canvas.SetActive(false);
        }
    }

    void Update()
    {
        if (computerPlayer == null) {
            computerPlayer = GameObject.FindGameObjectWithTag("ComputerPlayer");
        } else {
            transform.parent = computerPlayer.transform;
        }

        if (!IsServer)
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                ToggleFlashlight();
            }
        }
    }
}
