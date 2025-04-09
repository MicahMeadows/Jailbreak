using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 40;
        buttonStyle.padding = new RectOffset(20, 20, 20, 20);
        buttonStyle.alignment = TextAnchor.MiddleCenter;

        if (GUILayout.Button("Host", buttonStyle)) {
            networkManager.StartHost();
        }

        if (GUILayout.Button("Client", buttonStyle)) {
            var ip = "192.168.1.135";
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = ip;
            networkManager.StartClient();
        }

        if (GUILayout.Button("Server", buttonStyle)) {
            networkManager.StartServer();
        }

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