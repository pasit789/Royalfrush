using UnityEngine;
using RoyalFlush.Interaction;

namespace RoyalFlush.Dialogue
{
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [Header("Dialogue Content")]
        public string characterName;
        [TextArea(3, 10)]
        public string[] sentences;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        public void Interact()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(characterName, sentences);
            }
            else
            {
                Debug.LogWarning("DialogueManager is missing in the scene!");
            }
        }

        public void ShowInteractionPrompt()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.yellow; // Highlight
            }
        }

        public void HideInteractionPrompt()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor; // Revert color
            }
        }
    }
}
