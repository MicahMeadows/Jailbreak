using Unity.Netcode;
using UnityEngine;

public class HelloWorldManager : MonoBehaviour
{
    private NetworkManager networkManager;

    void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!networkManager.IsClient && !networkManager.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) networkManager.StartHost();
        if (GUILayout.Button("Client")) networkManager.StartClient();
        if (GUILayout.Button("Server")) networkManager.StartServer();
    }

    void StatusLabels()
    {
        var mode = networkManager.IsHost ?
            "Host" : networkManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            networkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    
}