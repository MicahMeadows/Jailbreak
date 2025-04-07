using UnityEngine;
using UnityEngine.Events;

public class ExitDoor : MonoBehaviour, IInteractable
{
    private bool canExit = false;
    public UnityEvent onExit;

    public void SetCanExit(bool value)
    {
        canExit = value;
    }

    public InteractResponse QueryInteraction()
    {
        if (canExit)
        {
            return new InteractResponse
            {
                maxRange = 3f,
                message = "Leave..."
            };
        }
        return new InteractResponse
        {
            maxRange = 3f,
            message = "Cannot leave yet..."
        };
    }

    public void Interact()
    {
        if (canExit)
        {
            onExit.Invoke();
        }
    }
}
