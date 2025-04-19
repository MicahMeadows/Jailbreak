using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private float openRot = 90f;
    [SerializeField] private float closedRot = 0f;
    public float speed = 5f;
    [SerializeField] private GameObject door;
    [SerializeField] private bool isOpen;

    public InteractResponse QueryInteraction()
    {
        return new InteractResponse()
        {
            maxRange = 3f,
            message = isOpen ? "Close Door" : "Open Door"
        };
    }

    public void Interact()
    {
        isOpen = !isOpen;
    }

    void Update()
    {
        float targetY = isOpen ? openRot : closedRot;
        Quaternion targetRotation = Quaternion.Euler(0, targetY, 0);

        float angleDiff = Quaternion.Angle(door.transform.localRotation, targetRotation);

        if (angleDiff < 0.1f)
        {
            door.transform.localRotation = targetRotation;
        }
        else
        {
            door.transform.localRotation = Quaternion.Lerp(door.transform.localRotation, targetRotation, speed * Time.deltaTime);
        }
    }


}
