using UnityEngine;

namespace RoyalFlush.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private Transform interactionPoint;
        [SerializeField] private float interactionRadius = 1f;
        [SerializeField] private LayerMask interactableMask;

        private IInteractable currentInteractable;

        private void Update()
        {
            CheckForInteractable();

            if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }

        private void CheckForInteractable()
        {
            Collider2D collider = Physics2D.OverlapCircle(interactionPoint.position, interactionRadius, interactableMask);

            if (collider != null)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();

                if (interactable != null && interactable != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.HideInteractionPrompt();
                    }
                    
                    currentInteractable = interactable;
                    currentInteractable.ShowInteractionPrompt();
                }
            }
            else
            {
                if (currentInteractable != null)
                {
                    currentInteractable.HideInteractionPrompt();
                    currentInteractable = null;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (interactionPoint == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
        }
    }
}
