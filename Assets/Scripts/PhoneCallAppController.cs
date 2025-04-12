using System.Collections;
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

    private string incomingCallSequenceName = "";
    private string incomingCallCallerId = "";

    private float callStartTime;
    private Coroutine callTimerCoroutine;

    void Start()
    {
        incomingCallPickupButton.onClick.AddListener(OnIncomingCallPickup);
    }

    void OnIncomingCallPickup()
    {
        PickupCall_ServerRPC(incomingCallSequenceName);
        incomingCallGroup.SetActive(false);
        callAppGroup.SetActive(true);
        callerIdText.text = incomingCallCallerId;

        // Start call timer
        callStartTime = Time.time;
        if (callTimerCoroutine != null)
            StopCoroutine(callTimerCoroutine);
        callTimerCoroutine = StartCoroutine(UpdateCallLengthTimer());

        incomingCallCallerId = "";
        incomingCallSequenceName = "";
    }

    private IEnumerator UpdateCallLengthTimer()
    {
        while (true)
        {
            float elapsed = Time.time - callStartTime;
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            callLength.text = $"{minutes:D2}:{seconds:D2}";
            yield return new WaitForSeconds(1f);
        }
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    public void PickupCall_ServerRPC(string sequenceName)
    {
        phonePlayer.PickupIncomingCall(sequenceName);
    }

    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    public void ShowCallPopup_ClientRPC(string callerId, string sequenceName)
    {
        if (!IsServer)
        {
            incomingCallGroup.SetActive(true);
            incomingCallerIdText.text = callerId;
            incomingCallSequenceName = sequenceName;
            incomingCallCallerId = callerId;
        }
    }
}
