using UnityEngine;
using UnityEngine.Events;
using RoyalFlush.Interaction;

namespace RoyalFlush.Dialogue
{
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [Header("Dialogue Content")]
        public string characterName;
        [TextArea(3, 10)]
        public string[] sentences;

        [Header("Dialogue Choices (Until Then Style - Optional)")]
        public DialogueChoice[] choices;

        [Header("Interaction Rules")]
        [Tooltip("If true, this dialogue can only be played once and cannot be triggered again.")]
        [SerializeField] private bool playOnlyOnce = true;
        [SerializeField] private UnityEvent onDialogueCompleted;

        [Header("Visual Feedback")]
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.7f, 1f);
        [SerializeField] private float highlightPulseSpeed = 2f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private bool hasBeenPlayed = false;
        private bool isHighlighted = false;

        public bool HasBeenPlayed => hasBeenPlayed;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void Update()
        {
            // Pulse highlight effect when player is near
            if (isHighlighted && spriteRenderer != null)
            {
                float pulse = Mathf.Sin(Time.time * highlightPulseSpeed) * 0.5f + 0.5f;
                spriteRenderer.color = Color.Lerp(originalColor, highlightColor, pulse * 0.4f);
            }
        }

        public void Interact()
        {
            if (playOnlyOnce && hasBeenPlayed)
            {
                return;
            }

            if (DialogueManager.Instance != null)
            {
                HideInteractionPrompt();
                // Pass this transform so the camera can focus on this NPC during dialogue
                DialogueManager.Instance.StartDialogue(characterName, sentences, choices, () =>
                {
                    hasBeenPlayed = true;
                    onDialogueCompleted?.Invoke();
                }, transform);
            }
            else
            {
                Debug.LogWarning("DialogueManager is missing in the scene!");
            }
        }

        public void ShowInteractionPrompt()
        {
            if (playOnlyOnce && hasBeenPlayed) return;

            isHighlighted = true;
        }

        public void HideInteractionPrompt()
        {
            isHighlighted = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor; // Revert color
            }
        }
    }
}
