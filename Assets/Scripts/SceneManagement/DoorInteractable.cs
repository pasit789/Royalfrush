using UnityEngine;
using RoyalFlush.Interaction;

namespace RoyalFlush.SceneManagement
{
    public class DoorInteractable : MonoBehaviour, IInteractable
    {
        public enum TransitionType
        {
            LoadNewScene,
            TeleportInSameScene
        }

        [Header("Transition Settings")]
        [SerializeField] private TransitionType transitionType = TransitionType.TeleportInSameScene;
        [SerializeField] private string targetSceneName = "";
        [SerializeField] private Transform targetSpawnPoint;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer promptRenderer;
        [SerializeField] private Color highlightColor = Color.yellow;
        private Color originalColor = Color.white;

        private void Awake()
        {
            if (promptRenderer == null)
            {
                promptRenderer = GetComponent<SpriteRenderer>();
            }

            if (promptRenderer != null)
            {
                originalColor = promptRenderer.color;
            }
        }

        public void Interact()
        {
            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogWarning("SceneTransitionManager is not in the scene!");
                return;
            }

            if (transitionType == TransitionType.LoadNewScene && !string.IsNullOrEmpty(targetSceneName))
            {
                SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
            }
            else if (transitionType == TransitionType.TeleportInSameScene && targetSpawnPoint != null)
            {
                SceneTransitionManager.Instance.TeleportPlayer(targetSpawnPoint);
            }
        }

        public void ShowInteractionPrompt()
        {
            if (promptRenderer != null)
            {
                promptRenderer.color = highlightColor;
            }
        }

        public void HideInteractionPrompt()
        {
            if (promptRenderer != null)
            {
                promptRenderer.color = originalColor;
            }
        }
    }
}
