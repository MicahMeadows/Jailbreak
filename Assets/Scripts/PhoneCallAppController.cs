using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCallAppController : NetworkBehaviour
{
    [SerializeField] private PhonePlayer phonePlayer;
    [SerializeField] private GameObject callAppGroup;
    [SerializeField] private TextMeshProUGUI callerIdText;
    [SerializeField] private TextMeshProUGUI callLength;
    [SerializeField] private Button hangupButton;
    [SerializeField] private RawImage contactImage;

    // Incoming Call Popup
    [SerializeField] private GameObject incomingCallGroup;
    [SerializeField] private TextMeshProUGUI incomingCallerIdText;
    [SerializeField] private Button incomingCallPickupButton;
    [SerializeField] private RawImage incomingCallContactImage;

    void Start()
    {
        incomingCallPickupButton.onClick.AddListener(OnIncomingCallPickup);
    }

    void OnIncomingCallPickup()
    {
        incomingCallGroup.SetActive(false);
        StopPhoneRing_ServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartPhoneRing_ServerRPC()
    {
        phonePlayer.StartPhoneRing();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopPhoneRing_ServerRPC()
    {
        phonePlayer.StopPhoneRing();
    }

    [ClientRpc(RequireOwnership = false)]
    public void CreateIncomingCall_ClientRPC(string clientId)
    {
        if (!IsServer)
        {
            CreateIncomingCall(clientId);
            StartPhoneRing_ServerRPC();
        }
    }

    public void CreateIncomingCall(string callerId)
    {
        incomingCallGroup.SetActive(true);
        incomingCallerIdText.text = callerId;
    }

}
