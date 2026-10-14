using UnityEngine;

namespace RoyalFlush.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float interactionCooldown = 0.2f; // Prevent rapid re-interaction

        private IInteractable currentInteractable;
        private InteractionPromptUI currentPromptUI;
        private float lastInteractionTime;

        private void Update()
        {
            // If currently in dialogue, ignore re-interaction so 'E' advances text instead
            if (RoyalFlush.Dialogue.DialogueManager.Instance != null && RoyalFlush.Dialogue.DialogueManager.Instance.IsInDialogue)
            {
                return;
            }

            // If phone is open, don't interact with world objects
            if (RoyalFlush.Phone.PhoneManager.Instance != null && RoyalFlush.Phone.PhoneManager.Instance.IsPhoneOpen)
            {
                return;
            }

            // Interact when pressing E near an interactable object (with cooldown)
            if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
            {
                if (Time.time - lastInteractionTime >= interactionCooldown)
                {
                    lastInteractionTime = Time.time;
                    currentInteractable.Interact();
                }
            }
        }

        // When entering an interaction trigger area
        private void OnTriggerEnter2D(Collider2D other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                currentInteractable = interactable;
                currentInteractable.ShowInteractionPrompt();

                // Try to find and show floating prompt UI
                InteractionPromptUI prompt = other.GetComponent<InteractionPromptUI>();
                if (prompt == null) prompt = other.GetComponentInChildren<InteractionPromptUI>();
                if (prompt != null)
                {
                    currentPromptUI = prompt;
                    currentPromptUI.Show();
                }
            }
        }

        // When leaving an interaction trigger area
        private void OnTriggerExit2D(Collider2D other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            
            if (interactable != null && interactable == currentInteractable)
            {
                currentInteractable.HideInteractionPrompt();
                currentInteractable = null;

                if (currentPromptUI != null)
                {
                    currentPromptUI.Hide();
                    currentPromptUI = null;
                }
            }
        }
    }
}
