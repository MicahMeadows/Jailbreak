using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct SweepingPoint
{
    public Vector3 endRotation;
    public float sweepTime;
    public float pauseTime;
}

public class SecurityCamera : NetworkBehaviour
{
    [SerializeField] private GameObject cam;
    [SerializeField] private string camName;
    [SerializeField] private List<SweepingPoint> sweepingPoints;

    private int currentSweepPoint = 0;
    private Camera camComponent;
    private RenderTexture renderTexture;
    public MeshRenderer screen;

    void Start()
    {
        camComponent = cam.GetComponent<Camera>();
        if (camComponent == null)
        {
            Debug.LogError("No Camera component found on cam GameObject!");
            return;
        }

        // Create and assign a dynamic RenderTexture
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (sweepingPoints.Count > 0)
            {
                StartCoroutine(SweepRoutine());
            }

            // Make lower res on pc where multiple are in play
            if (screen != null)
            {
                cam.SetActive(true);
                renderTexture = new RenderTexture(240, 135, 16);
                renderTexture.name = $"{camName}_RenderTexture";
                camComponent.targetTexture = renderTexture;
                screen.material.mainTexture = renderTexture;
                screen.material.SetColor("_EmissiveColor", Color.white);
                screen.material.EnableKeyword("_Emission");
            }
            
        }
        else 
        {
            renderTexture = new RenderTexture(1280, 720, 16);
            renderTexture.name = $"{camName}_RenderTexture";
            camComponent.targetTexture = renderTexture;
        }
        
    }

    public void SetActive(bool value)
    {
        cam.SetActive(value);
    }

    private IEnumerator SweepRoutine()
    {
        while (true)
        {
            SweepingPoint targetPoint = sweepingPoints[currentSweepPoint];
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = Quaternion.Euler(targetPoint.endRotation);

            float elapsedTime = 0f;
            while (elapsedTime < targetPoint.sweepTime)
            {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / targetPoint.sweepTime);
                transform.rotation = Quaternion.Euler(
                    transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.y,
                    0
                ); // Lock Z-axis rotation
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.rotation = endRotation;
            yield return new WaitForSeconds(targetPoint.pauseTime);

            currentSweepPoint = (currentSweepPoint + 1) % sweepingPoints.Count;
        }
    }

    public string GetCamName()
    {
        return camName;
    }

    // ✅ Get the camera's RenderTexture
    public RenderTexture GetCamTexture()
    {
        return renderTexture;
    }
}
