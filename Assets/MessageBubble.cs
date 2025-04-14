using UnityEngine;
using UnityEngine.UI;

public class MessageBubble : MonoBehaviour
{
    public bool isOutgoing = false;

    public VerticalLayoutGroup vlg;

    const int TEXT_BOX_WIDTH = 800;

    void Start()
    {
        float screenWidth = Screen.width;
        int margin = Mathf.RoundToInt(screenWidth - TEXT_BOX_WIDTH);

        int leftPadding = isOutgoing ? margin : 0;
        int rightPadding = isOutgoing ? 0 : margin;

        vlg.padding.left = leftPadding;
        vlg.padding.right = rightPadding;

        LayoutRebuilder.ForceRebuildLayoutImmediate(vlg.GetComponent<RectTransform>());
    }
}
