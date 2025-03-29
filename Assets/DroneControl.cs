using Unity.Netcode;
using UnityEngine;

public class DroneControl : NetworkBehaviour
{
    [SerializeField] private GameObject droneCam;


    public override void OnNetworkSpawn()
    {
        // droneCam = transform.Find("DroneCamera").gameObject;
        droneCam.SetActive(false);
        if (!IsServer)
        {
            droneCam.SetActive(true);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
