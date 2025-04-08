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
            Renderer renderer = gameObject.GetComponent<Renderer>();
            Material[] mats = renderer.materials; // This is a copy
            mats[1] = emptyShelfMaterial;         // Modify the copy
            renderer.materials = mats;

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
            message = "Steal"
        };
    }
}
