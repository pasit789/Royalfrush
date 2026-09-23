using UnityEngine;

namespace RoyalFlush.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        private IInteractable currentInteractable;

        private void Update()
        {
            // ถ้ามีของให้กดได้ และผู้เล่นกดปุ่ม E
            if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }

        // เมื่อเดินไปชนโดนกล่องที่มี Trigger
        private void OnTriggerEnter2D(Collider2D other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                currentInteractable = interactable;
                currentInteractable.ShowInteractionPrompt();
            }
        }

        // เมื่อเดินออกมาห่างจากกล่อง
        private void OnTriggerExit2D(Collider2D other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            
            if (interactable != null && interactable == currentInteractable)
            {
                currentInteractable.HideInteractionPrompt();
                currentInteractable = null;
            }
        }
    }
}
