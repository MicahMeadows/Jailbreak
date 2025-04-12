using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCallAppController : MonoBehaviour
{
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

    // with callback for on pickup
    public void CreateIncomingCall(string callerId)
    {
        incomingCallGroup.SetActive(true);
        incomingCallerIdText.text = callerId;
    }
}
