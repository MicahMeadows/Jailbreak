using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class FoodShelf : MonoBehaviour, IInteractable
{
    private bool itemStolen = false;
    public UnityEvent onStolen;
    private float maxInteractDistance = 2f;
    public Material emptyShelfMaterial;

    public void Interact()
    {
        if (itemStolen == false)
        {
            Debug.Log("Stole some food!");
            itemStolen = true;
            this.gameObject.GetComponent<Renderer>().material = emptyShelfMaterial;
            onStolen.Invoke();
        }
    }

    public InteractResponse QueryInteraction()
    {
        if (itemStolen)
        {
            return new InteractResponse
            {
                maxRange = maxInteractDistance,
                message = ""
            };
        }
        return new InteractResponse
        {
            maxRange = maxInteractDistance,
            message = "You can interact with the food shelf."
        };
    }
}
