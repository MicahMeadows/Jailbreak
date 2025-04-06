using UnityEngine;

public class FoodShelf : MonoBehaviour, IInteractable
{
    bool shelfEmpty = false;

    public void Interact()
    {
        Debug.Log("Stole some food!");
        shelfEmpty = true;
    }

    public InteractResponse QueryInteraction()
    {
        if (shelfEmpty)
        {
            return new InteractResponse
            {
                maxRange = 5f,
                message = "The food shelf is empty."
            };
        }
        return new InteractResponse
        {
            maxRange = 5f,
            message = "You can interact with the food shelf."
        };
    }
}
