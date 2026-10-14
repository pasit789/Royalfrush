using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalFlush.Player;

namespace RoyalFlush.Phone
{
    public class PhoneManager : MonoBehaviour
    {
        public static PhoneManager Instance { get; private set; }

        [Header("Keybinding")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Header("Phone Frame & Sliding Animation")]
        [SerializeField] private RectTransform phonePanelRect;
        [SerializeField] private Vector2 hiddenAnchoredPos = new Vector2(0, -900f);
        [SerializeField] private Vector2 visibleAnchoredPos = new Vector2(0, 0f);
        [SerializeField] private float slideDuration = 0.35f;

        [Header("Status Bar")]
        [SerializeField] private TextMeshProUGUI clockText;
        [SerializeField] private bool useSystemTime = true;
        [SerializeField] private string simulatedTime = "07:30 AM";

        [Header("Notification Alert")]
        [SerializeField] private GameObject notificationBadge;
        [SerializeField] private AudioSource notificationAudioSource;
        [SerializeField] private AudioClip notificationSound;

        [Header("Chat UI View")]
        [SerializeField] private GameObject chatViewScreen;
        [SerializeField] private TextMeshProUGUI chatContactNameText;
        [SerializeField] private Transform messageContainer;
        [SerializeField] private GameObject chatBubblePrefab;
        [SerializeField] private ScrollRect chatScrollRect;

        [Header("Reply UI View")]
        [SerializeField] private GameObject replyContainer;
        [SerializeField] private Button replyButtonPrefab;

        [Header("Conversations Data")]
        [SerializeField] private List<ContactConversation> conversations = new List<ContactConversation>();

        private bool isOpen = false;
        private Coroutine slideCoroutine;
        private ContactConversation activeConversation;

        public bool IsPhoneOpen => isOpen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (phonePanelRect != null)
            {
                phonePanelRect.anchoredPosition = hiddenAnchoredPos;
            }

            if (notificationBadge != null)
            {
                notificationBadge.SetActive(false);
            }
        }

        private void Start()
        {
            UpdateNotificationBadge();

            // Auto-open first conversation if available
            if (conversations.Count > 0)
            {
                OpenConversation(conversations[0]);
            }
        }

        private void Update()
        {
            // Toggle Phone with key (e.g. Tab)
            if (Input.GetKeyDown(toggleKey))
            {
                // Don't open phone during active world dialogue
                if (RoyalFlush.Dialogue.DialogueManager.Instance != null && RoyalFlush.Dialogue.DialogueManager.Instance.IsInDialogue)
                {
                    return;
                }

                TogglePhone();
            }

            UpdateClock();
        }

        public void TogglePhone()
        {
            SetPhoneOpen(!isOpen);
        }

        public void SetPhoneOpen(bool open)
        {
            isOpen = open;

            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            Vector2 targetPos = isOpen ? visibleAnchoredPos : hiddenAnchoredPos;
            slideCoroutine = StartCoroutine(SlideRoutine(targetPos));

            // Lock or unlock player movement
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.SetMovementEnabled(!isOpen);
            }

            if (isOpen && activeConversation != null)
            {
                activeConversation.hasUnread = false;
                UpdateNotificationBadge();
            }
        }

        private IEnumerator SlideRoutine(Vector2 targetPos)
        {
            if (phonePanelRect == null) yield break;

            Vector2 startPos = phonePanelRect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
                phonePanelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            phonePanelRect.anchoredPosition = targetPos;
        }

        private void UpdateClock()
        {
            if (clockText == null) return;

            if (useSystemTime)
            {
                clockText.text = System.DateTime.Now.ToString("hh:mm tt");
            }
            else
            {
                clockText.text = simulatedTime;
            }
        }

        public void OpenConversation(ContactConversation contact)
        {
            activeConversation = contact;
            if (contact == null) return;

            contact.hasUnread = false;
            UpdateNotificationBadge();

            if (chatContactNameText != null)
            {
                chatContactNameText.text = contact.contactName;
            }

            RefreshChatMessagesUI();
            RefreshReplyButtonsUI();
        }

        private void RefreshChatMessagesUI()
        {
            if (messageContainer == null || activeConversation == null) return;

            // Clear old bubbles
            foreach (Transform child in messageContainer)
            {
                Destroy(child.gameObject);
            }

            // Instantiate bubbles
            foreach (var msg in activeConversation.messages)
            {
                SpawnBubble(msg);
            }

            StartCoroutine(ScrollToBottom());
        }

        private void SpawnBubble(ChatMessageData data)
        {
            if (chatBubblePrefab == null || messageContainer == null) return;

            GameObject obj = Instantiate(chatBubblePrefab, messageContainer);
            ChatBubbleUI bubble = obj.GetComponent<ChatBubbleUI>();
            if (bubble != null)
            {
                bubble.Setup(data);
            }
        }

        private void RefreshReplyButtonsUI()
        {
            if (replyContainer == null) return;

            // Clear old reply buttons
            foreach (Transform child in replyContainer.transform)
            {
                Destroy(child.gameObject);
            }

            if (activeConversation == null || activeConversation.pendingReplyOptions == null || activeConversation.pendingReplyOptions.Count == 0)
            {
                replyContainer.SetActive(false);
                return;
            }

            replyContainer.SetActive(true);

            foreach (var replyOption in activeConversation.pendingReplyOptions)
            {
                var option = replyOption;
                if (replyButtonPrefab == null) break;

                Button btn = Instantiate(replyButtonPrefab, replyContainer.transform);
                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = option.replyLabel;
                }

                btn.onClick.AddListener(() => OnSelectReply(option));
            }
        }

        private void OnSelectReply(ChatReplyOption chosenOption)
        {
            if (activeConversation == null) return;

            // Add player's reply message
            string timeStr = System.DateTime.Now.ToString("hh:mm tt");
            ChatMessageData playerMsg = new ChatMessageData("Me", chosenOption.fullMessageText, true, timeStr);
            activeConversation.messages.Add(playerMsg);
            SpawnBubble(playerMsg);

            // Clear options
            activeConversation.pendingReplyOptions.Clear();
            RefreshReplyButtonsUI();
            StartCoroutine(ScrollToBottom());

            // If there's an incoming response, trigger it after a short delay
            if (chosenOption.incomingResponseAfterReply != null && !string.IsNullOrEmpty(chosenOption.incomingResponseAfterReply.messageText))
            {
                StartCoroutine(DelayedIncomingResponse(chosenOption.incomingResponseAfterReply));
            }
        }

        private IEnumerator DelayedIncomingResponse(ChatMessageData response)
        {
            yield return new WaitForSeconds(1.2f);

            if (activeConversation != null)
            {
                response.timeString = System.DateTime.Now.ToString("hh:mm tt");
                activeConversation.messages.Add(response);
                SpawnBubble(response);
                PlayNotificationSound();
                StartCoroutine(ScrollToBottom());
            }
        }

        public void ReceiveNewMessage(string contactId, string senderName, string messageText, ChatReplyOption[] followUpChoices = null)
        {
            ContactConversation contact = conversations.Find(c => c.contactId == contactId);
            if (contact == null)
            {
                contact = new ContactConversation { contactId = contactId, contactName = senderName };
                conversations.Add(contact);
            }

            string timeStr = System.DateTime.Now.ToString("hh:mm tt");
            ChatMessageData newMsg = new ChatMessageData(senderName, messageText, false, timeStr);
            contact.messages.Add(newMsg);

            if (followUpChoices != null && followUpChoices.Length > 0)
            {
                contact.pendingReplyOptions.Clear();
                contact.pendingReplyOptions.AddRange(followUpChoices);
            }

            if (!isOpen || activeConversation != contact)
            {
                contact.hasUnread = true;
                UpdateNotificationBadge();
                PlayNotificationSound();
            }
            else
            {
                SpawnBubble(newMsg);
                RefreshReplyButtonsUI();
                StartCoroutine(ScrollToBottom());
            }
        }

        private void PlayNotificationSound()
        {
            if (notificationAudioSource != null && notificationSound != null)
            {
                notificationAudioSource.PlayOneShot(notificationSound);
            }
        }

        private void UpdateNotificationBadge()
        {
            if (notificationBadge == null) return;

            bool anyUnread = conversations.Exists(c => c.hasUnread);
            notificationBadge.SetActive(anyUnread);
        }

        private IEnumerator ScrollToBottom()
        {
            yield return new WaitForEndOfFrame();
            if (chatScrollRect != null)
            {
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
