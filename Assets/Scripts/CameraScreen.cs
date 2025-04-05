using UnityEngine;

public class CameraScreen : MonoBehaviour
{
    public SecurityCamera cameraToView;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (meshRenderer && cameraToView)
        {
            // meshRenderer.material.mainTexture = cameraToView.GetCamTexture();
            meshRenderer.material.mainTexture = cameraToView.GetBackupCamTexture();
        }
    }
}
