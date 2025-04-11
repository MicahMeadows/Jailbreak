using Unity.VisualScripting;
using UnityEngine;

public class PhotoTarget : MonoBehaviour
{
    public string targetName;
    public int maxDistance;
    [SerializeField] private BoxCollider targetCollider;

    public int GetMaxDistance()
    {
        return maxDistance;
    }

    public Transform GetTargetColliderTransform()
    {
        return targetCollider.transform;
    }

    public string GetName()
    {
        return targetName;
    }

}
