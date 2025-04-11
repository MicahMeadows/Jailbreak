using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class SetupDevices : MonoBehaviour
{
    public string serverIp = "192.168.1.134";
    public ushort port = 8088;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(serverIp, port);
            // NetworkManager.Singleton.StartClient();

            StartCoroutine(RetryConnection(999, 10f)); // try 5 times, wait 3 seconds between attempts

        }
    }

    IEnumerator RetryConnection(int maxAttempts, float delaySeconds)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(serverIp, port);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Debug.Log($"Attempt {attempt + 1} to connect...");
            NetworkManager.Singleton.StartClient();

            float timer = 0f;
            while (timer < delaySeconds)
            {
                if (NetworkManager.Singleton.IsConnectedClient)
                {
                    Debug.Log("Connected!");
                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            NetworkManager.Singleton.Shutdown(); // clean up after failed attempt
        }

        Debug.LogWarning("Failed to connect after max attempts.");
    }

}
