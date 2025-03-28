using UnityEngine;

public class PhonePlayer : MonoBehaviour
{
    private GameObject computerPlayer;

    void Start()
    {
        
    }

    void Update()
    {
        computerPlayer = GameObject.FindGameObjectWithTag("ComputerPlayer");
        if (computerPlayer != null)
        {
            transform.parent = computerPlayer.transform;
        }
        
    }
}
