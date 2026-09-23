using UnityEngine;

namespace RoyalFlush.Interaction
{
    public class TestInteractable : MonoBehaviour, IInteractable
    {
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
            Debug.Log($"Interacted with {gameObject.name}!");
            if (spriteRenderer != null)
            {
                // Toggle color to show interaction worked
                spriteRenderer.color = spriteRenderer.color == originalColor ? Color.green : originalColor;
            }
        }

        public void ShowInteractionPrompt()
        {
            Debug.Log($"Press E to interact with {gameObject.name}");
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.yellow; // Highlight when near
            }
        }

        public void HideInteractionPrompt()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor; // Revert when far
            }
        }
    }
}
