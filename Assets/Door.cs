using UnityEngine;

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
        Vector3 curRot = door.transform.localEulerAngles;
        if (isOpen)
        {
            if (curRot.y < openRot)
            {
                door.transform.localEulerAngles = Vector3.Lerp(curRot, new Vector3(curRot.x, openRot, curRot.z), speed * Time.deltaTime);
            }
        }
        else
        {
            if (curRot.y > closedRot)
            {
                door.transform.localEulerAngles = Vector3.Lerp(curRot, new Vector3(curRot.x, closedRot, curRot.z), speed * Time.deltaTime);
            }
        }
    }
}
