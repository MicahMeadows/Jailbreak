using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBubble : MonoBehaviour
{
    public bool isOutgoing = false;
    public string message = "";

    public VerticalLayoutGroup vlg;

    // const int TEXT_BOX_WIDTH = 800;
    const int PADDING = 30;

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private RawImage bubble;
    [SerializeField] private RectTransform messageRect;
    [SerializeField] private LayoutElement messageLayoutElement;

    Color OUTGOING_COLOR = new Color(0, 131f/255f, 255f/255f);
    Color INCOMING_COLOR = new Color(140f/255f, 140f/255f, 140f/255f);

    static int MAX_WIDTH = 700;

    public void SetMessage(string message, bool outgoing)
    {
        isOutgoing = outgoing;
        this.message = message;

        UpdateMessageBubble();
    }

    private void UpdateMessageBubble()
    {
        messageText.text = message;
        bubble.color = isOutgoing ? OUTGOING_COLOR : INCOMING_COLOR;

        LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.GetComponent<RectTransform>());

        StartCoroutine(CheckAndApplyLayout());
    }

    private IEnumerator CheckAndApplyLayout()
    {
        yield return null; // wait one frame for layout to update

        var messagesScrollContent = transform.parent.GetComponent<RectTransform>();
        float currentWidth = messageRect.rect.width;

        Debug.Log("Current width: " + currentWidth);
        if (currentWidth > MAX_WIDTH)
        {
            messageLayoutElement.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(messagesScrollContent);
        }

        StartCoroutine(LateUpdateMrgins());

    }
    
    private IEnumerator LateUpdateMrgins()
    {
        yield return null;
        UpdateMargins();
    }

    private void UpdateMargins()
    {
        float currentWidth = messageRect.rect.width;

        float screenWidth = Screen.width;
        int margin = Mathf.RoundToInt(screenWidth - currentWidth) - PADDING;
        Debug.Log($"WID: {screenWidth} - {currentWidth} = {margin}");

        int leftPadding = isOutgoing ? margin : 0;
        int rightPadding = isOutgoing ? 0 : margin;

        vlg.padding.left = leftPadding;
        vlg.padding.right = rightPadding;

        var messageScrollContent = transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageScrollContent);
    }
    void Start()
    {
        // UpdateMessageBubble();
    }
}
