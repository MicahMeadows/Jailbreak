using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelloWorldManager : MonoBehaviour
{
    private NetworkManager networkManager;
    public bool loadScene2;

    void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
    }

    void Update()
    {
        if (loadScene2)
        {
            loadScene2 = false;
            if (networkManager.IsHost || networkManager.IsServer)
            {
                networkManager.SceneManager.LoadScene("Scene2", LoadSceneMode.Single);
            }
            else
            {
                networkManager.SceneManager.LoadScene("Scene1", LoadSceneMode.Single);
            }
        }
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