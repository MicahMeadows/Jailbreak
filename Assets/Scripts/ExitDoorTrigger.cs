using Unity.VisualScripting;
using UnityEngine;

public class ExitDoorTrigger : MonoBehaviour
{
    private HomeBaseLevelManager levelManager;

    void Start()
    {
        levelManager = FindFirstObjectByType<HomeBaseLevelManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ComputerPlayer"))
        {
            levelManager.ExitDoorTriggerEntered();
        }
    }
}
