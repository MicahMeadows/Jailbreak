using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextMessageItem : MonoBehaviour
{
    private bool playerMessage;

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private RawImage textBubble;


    public void SetText(string message, bool playerMessage)
    {
        messageText.text = message;
        // textBubble.GetComponent<RectTransform>().right = playerMessage ? 20 : 200;
        // textBubble.GetComponent<RectTransform>().left = playerMessage ? 200 : 200;

    }
}
