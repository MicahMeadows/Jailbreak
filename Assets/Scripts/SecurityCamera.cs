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

    void Start()
    {
        cam.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (sweepingPoints.Count > 0)
            {
                StartCoroutine(SweepRoutine());
            }
        }
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
                ); // Ensure local rotation is applied
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.rotation = endRotation; // Ensure exact final position
            yield return new WaitForSeconds(targetPoint.pauseTime);

            currentSweepPoint = (currentSweepPoint + 1) % sweepingPoints.Count;
        }
    }

    public string GetCamName()
    {
        return camName;
    }

    public void EnableCamera(bool value)
    {
        cam.SetActive(value);
    }
}
