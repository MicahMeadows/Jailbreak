using System;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [SerializeField] private GameObject cam;
    [SerializeField] private string camName;

    void Start()
    {
        cam.SetActive(false);
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
