using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject messageImageObject;
    [SerializeField] private GameObject messageTextObject;
    [SerializeField] private GameObject messageLandscapeImageObject;
    [SerializeField] private Button bubbleButton;
    private Texture2D image;
    private bool isLandscape;

    Color OUTGOING_COLOR = new Color(0, 131f/255f, 255f/255f);
    Color INCOMING_COLOR = new Color(140f/255f, 140f/255f, 140f/255f);

    static int MAX_WIDTH = 800;

    public void SetMessage(string message, bool outgoing, Texture2D image, bool isLandscape, Action onTap)
    {
        isOutgoing = outgoing;
        this.message = message;
        this.image = image;
        this.isLandscape = isLandscape;

        bubbleButton.onClick.RemoveAllListeners();
        bubbleButton.onClick.AddListener(() => onTap?.Invoke());

        UpdateMessageBubble();
    }

    private void UpdateMessageBubble()
    {
        if (image == null)
        {
            messageTextObject.SetActive(true);
            messageImageObject.SetActive(false);
            messageText.text = message;
            bubble.color = isOutgoing ? OUTGOING_COLOR : INCOMING_COLOR;
            LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.GetComponent<RectTransform>());
            StartCoroutine(CheckAndApplyLayout());
        }
        else
        {
            messageTextObject.SetActive(false);
            if (!isLandscape)
            {
                messageImageObject.SetActive(true);
                messageImageObject.GetComponent<RawImage>().texture = image;
            }
            else
            {
                messageLandscapeImageObject.SetActive(true);
                messageLandscapeImageObject.GetComponent<RawImage>().texture = image;
            }
            StartCoroutine(CheckAndApplyLayout());
        }

        
    }

    private IEnumerator CheckAndApplyLayout()
    {
        yield return null; // wait one frame for layout to update

        var messagesScrollContent = transform.parent.GetComponent<RectTransform>();
        float currentWidth = messageRect.rect.width;

        if (currentWidth > MAX_WIDTH && this.image == null)
        {
            messageLayoutElement.enabled = true;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesScrollContent);

        StartCoroutine(LateUpdateMrgins());

    }
    
    private IEnumerator LateUpdateMrgins()
    {
        yield return null;
        UpdateMargins();
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.GetComponent<RectTransform>());
    }

    private void UpdateMargins()
    {
        var rect = isLandscape ? messageLandscapeImageObject.GetComponent<RectTransform>() : messageImageObject.GetComponent<RectTransform>();
        float currentWidth = image == null ? messageRect.rect.width : rect.rect.width;

        float screenWidth = Screen.width;
        int margin = Mathf.RoundToInt(screenWidth - currentWidth) - PADDING;

        int leftPadding = isOutgoing ? margin : 0;
        int rightPadding = isOutgoing ? 0 : margin;

        vlg.padding.left = leftPadding;
        vlg.padding.right = rightPadding;

        var messageScrollContent = transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageScrollContent);

        StartCoroutine(LatRenderBubble());
    }

    private IEnumerator LatRenderBubble()
    {
        yield return null;

        bubble.enabled = false;
        yield return null;

        if (this.image == null) // only re-enable the bubble if there is no image basically only for text
        {
            bubble.enabled = true;
        }
    }


    void Start()
    {
        // UpdateMessageBubble();
    }
}
