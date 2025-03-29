using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PhonePlayer : NetworkBehaviour
{
    private GameObject computerPlayer = null;
    private GameObject cube;
    private GameObject canvas;


    public NetworkVariable<bool> isVisible = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public void ToggleFlashlight()
    {
        isVisible.Value = !isVisible.Value;
    }

    public override void OnNetworkSpawn()
    {
        cube = transform.Find("Cube").gameObject;
        canvas = GetComponentInChildren<Canvas>().gameObject;
        base.OnNetworkSpawn();
        if (IsServer)
        {
            canvas.SetActive(false);
        }
        if (!IsServer)
        {
            isVisible.Value = true;
        }
        isVisible.OnValueChanged += OnIsVisibleChanged;
    }

    private void OnIsVisibleChanged(bool prev, bool cur)
    {
        SetVisibility();
    }

    private void SetVisibility()
    {
        cube.SetActive(isVisible.Value);
    }

    void Update()
    {
        if (computerPlayer == null) {
            computerPlayer = GameObject.FindGameObjectWithTag("ComputerPlayer");
        } else {
            transform.parent = computerPlayer.transform;
        }

        if (!IsServer)
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                isVisible.Value = !isVisible.Value;
                Debug.Log($"Got key and set isVisible to {isVisible.Value}");
            }
        }
    }
}
