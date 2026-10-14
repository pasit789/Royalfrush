using UnityEngine;
using RoyalFlush.Dialogue;

namespace RoyalFlush.SceneManagement
{
    [RequireComponent(typeof(Collider2D))]
    public class ExitTrigger2D : MonoBehaviour
    {
        [Header("Target Scene")]
        [SerializeField] private string targetSceneName = "SchoolHallway";
        [SerializeField] private string targetSpawnPointName = "SpawnPoint_FromClassroom";

        [Header("Story Requirements")]
        [Tooltip("If true, requires the player to finish the classmate dialogue before leaving.")]
        [SerializeField] private bool requireDialogueComplete = true;
        [SerializeField] private DialogueTrigger prerequisiteDialogue;
        [SerializeField] private string lockedMessage = "I shouldn't leave just yet. I should talk to my classmate first.";

        [Header("Visual Indicator")]
        [SerializeField] private SpriteRenderer doorIndicator;
        [SerializeField] private float pulseSpeed = 2f;
        private Color indicatorOriginalColor;

        private bool isTransitioning = false;
        private bool playerInRange = false;

        private void Awake()
        {
            if (doorIndicator != null)
            {
                indicatorOriginalColor = doorIndicator.color;
            }
        }

        private void Update()
        {
            // Pulse door indicator when player is in range
            if (playerInRange && doorIndicator != null)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f;
                Color c = indicatorOriginalColor;
                c.a = pulse;
                doorIndicator.color = c;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTransitioning) return;
            if (!other.CompareTag("Player") && !other.name.Contains("Player")) return;

            playerInRange = true;

            // Check if prerequisite dialogue is required
            if (requireDialogueComplete && prerequisiteDialogue != null && !prerequisiteDialogue.HasBeenPlayed)
            {
                if (DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue)
                {
                    DialogueManager.Instance.StartDialogue("", new string[] { lockedMessage });
                }
                return;
            }

            // Start transition to new scene with spawn point
            isTransitioning = true;
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(targetSceneName, targetSpawnPointName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player") && !other.name.Contains("Player")) return;
            
            playerInRange = false;
            if (doorIndicator != null)
            {
                doorIndicator.color = indicatorOriginalColor;
            }
        }
    }
}
