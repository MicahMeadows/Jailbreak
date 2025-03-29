using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PhonePlayer : NetworkBehaviour
{
    private GameObject computerPlayer = null;
    private GameObject cube;

    public NetworkVariable<bool> isVisible = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        isVisible.OnValueChanged += OnIsVisibleChanged;
    }

    private void OnIsVisibleChanged(bool prev, bool cur)
    {
        SetVisibility();
    }

    void Start()
    {
        cube = transform.Find("Cube").gameObject;
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

        if (IsClient)
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                isVisible.Value = !isVisible.Value;
                Debug.Log($"Got key and set isVisible to {isVisible.Value}");
            }
        }
    }
}
