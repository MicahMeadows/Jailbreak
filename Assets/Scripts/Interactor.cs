using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public struct InteractResponse
{
    public float maxRange;
    public string message;
}

public interface IInteractable
{
    InteractResponse QueryInteraction();
    void Interact();
}

public class Interactor : MonoBehaviour
{
    public Transform interactorTransform;
    public float interactorRange;
    public TextMeshProUGUI interactText;

    void Update()
    {
        var mask = ~( (1 << LayerMask.NameToLayer("PlayerHidden")) |
              (1 << LayerMask.NameToLayer("SecurityCamCheck")));

        Ray r = new Ray(interactorTransform.position, interactorTransform.forward);
        if (Physics.Raycast(r, out RaycastHit hit, interactorRange, mask))
        {
            if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
            {
                InteractResponse response = interactable.QueryInteraction();
                if (hit.distance < response.maxRange)
                {
                    if (response.message != null)
                    {
                        interactText.text = response.message;
                    }
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactable.Interact();
                    }
                }
                else
                {
                    HandleFailedInteraction();
                }
            }
            else
            {
                HandleFailedInteraction();
            }
        }
        else
        {
            HandleFailedInteraction();
        }
    }

    void HandleFailedInteraction()
    {
        interactText.text = "";
    }
}
