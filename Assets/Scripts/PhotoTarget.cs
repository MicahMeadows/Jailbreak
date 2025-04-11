using Unity.VisualScripting;
using UnityEngine;

public class PhotoTarget : MonoBehaviour
{
    public string targetName;
    [SerializeField] private BoxCollider targetCollider;

    public Transform GetTargetColliderTransform()
    {
        return targetCollider.transform;
    }

    public string GetName()
    {
        return targetName;
    }

}
