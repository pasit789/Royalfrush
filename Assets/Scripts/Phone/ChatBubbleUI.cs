using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoyalFlush.Phone
{
    public class ChatBubbleUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image bubbleBackground;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Appearance Settings")]
        [SerializeField] private Color playerBubbleColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color contactBubbleColor = new Color(0.3f, 0.3f, 0.35f, 1f);

        public void Setup(ChatMessageData data)
        {
            if (messageText != null)
            {
                messageText.text = data.messageText;
            }

            if (timeText != null)
            {
                timeText.text = data.timeString;
            }

            if (bubbleBackground != null)
            {
                bubbleBackground.color = data.isFromPlayer ? playerBubbleColor : contactBubbleColor;
            }

            // Adjust alignment depending on whether it's player or contact
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                if (data.isFromPlayer)
                {
                    rect.pivot = new Vector2(1f, 0.5f);
                }
                else
                {
                    rect.pivot = new Vector2(0f, 0.5f);
                }
            }
        }
    }
}
