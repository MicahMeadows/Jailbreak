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
    [SerializeField] private string camName;
    [SerializeField] private List<SweepingPoint> sweepingPoints;

    public Vector2 cameraScreenRes = new Vector2(240, 135);

    private int currentSweepPoint = 0;
    private Camera camComponent;
    private Camera backupCamComponent;
    private RenderTexture renderTexture;
    private RenderTexture smallRenderTexture;

    public override void OnNetworkSpawn()
    {
        var cameras = GetComponentsInChildren<Camera>();
        camComponent = cameras[0];
        backupCamComponent = cameras[1];
        if (IsServer)
        {
            if (sweepingPoints.Count > 0)
            {
                StartCoroutine(SweepRoutine());
            }
        }

        // If phone client create a high res texture for viewing in phone app
        if (!IsServer)
        {
            renderTexture = new RenderTexture(1280, 720, 16);
            renderTexture.name = $"{camName}_RenderTexture";
            camComponent.targetTexture = renderTexture;
        }

        // Create fallback texture for displaying on smaller screen where several cameras might be on
        smallRenderTexture = new RenderTexture((int)cameraScreenRes.x, (int)cameraScreenRes.y, 16);
        smallRenderTexture.name = $"{camName}_Small_RenderTexture";
        backupCamComponent.targetTexture = smallRenderTexture;
        backupCamComponent.enabled = true;
    }

    public void SetActive(bool value)
    {
        // cam.SetActive(value);
        camComponent.enabled = value;
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

    public RenderTexture GetCamTexture()
    {
        return renderTexture;
    }

    public RenderTexture GetBackupCamTexture()
    {
        return smallRenderTexture;
    }
}
