using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoyalFlush.Player;
using RoyalFlush.CameraSystem;

namespace RoyalFlush.SceneManagement
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("UI Fade Settings")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.8f;

        private bool isTransitioning = false;
        
        // Store the spawn point name for the next scene
        private static string pendingSpawnPointName = "";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 1f;
                StartCoroutine(Fade(0f));
            }

            // Listen for scene loaded events to reposition player
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Transition to a new scene with optional spawn point name.
        /// </summary>
        public void TransitionToScene(string sceneName, string spawnPointName = "")
        {
            if (isTransitioning) return;
            pendingSpawnPointName = spawnPointName;
            StartCoroutine(TransitionRoutine(sceneName));
        }

        /// <summary>
        /// Teleport the player within the same scene with a fade effect.
        /// </summary>
        public void TeleportPlayer(Transform targetSpawnPoint)
        {
            if (isTransitioning || targetSpawnPoint == null) return;
            StartCoroutine(TeleportRoutine(targetSpawnPoint.position));
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            isTransitioning = true;

            // Lock player movement
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.SetMovementEnabled(false);
            }

            yield return StartCoroutine(Fade(1f));

            // Load target scene
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
            {
                yield return null;
            }

            // Wait a frame for objects to initialize
            yield return null;
            yield return new WaitForSeconds(0.15f);

            yield return StartCoroutine(Fade(0f));

            // Re-enable player movement
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.SetMovementEnabled(true);
            }

            isTransitioning = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Find and position player at spawn point
            if (!string.IsNullOrEmpty(pendingSpawnPointName))
            {
                GameObject spawnObj = GameObject.Find(pendingSpawnPointName);
                if (spawnObj != null && PlayerMovement.Instance != null)
                {
                    PlayerMovement.Instance.TeleportTo(spawnObj.transform.position);
                }
            }
            else
            {
                // Look for a default spawn point
                GameObject defaultSpawn = GameObject.FindWithTag("SpawnPoint");
                if (defaultSpawn == null) defaultSpawn = GameObject.Find("SpawnPoint");
                if (defaultSpawn != null && PlayerMovement.Instance != null)
                {
                    PlayerMovement.Instance.TeleportTo(defaultSpawn.transform.position);
                }
            }

            // Snap camera to player position immediately after scene load
            CameraFollow2D cam = FindAnyObjectByType<CameraFollow2D>();
            if (cam != null)
            {
                cam.SnapToTarget();
            }

            pendingSpawnPointName = "";
        }

        private IEnumerator TeleportRoutine(Vector3 newPosition)
        {
            isTransitioning = true;

            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.SetMovementEnabled(false);
            }

            yield return StartCoroutine(Fade(1f));

            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.TeleportTo(newPosition);
            }

            yield return new WaitForSeconds(0.15f);

            yield return StartCoroutine(Fade(0f));

            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.SetMovementEnabled(true);
            }

            isTransitioning = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (fadeCanvasGroup == null) yield break;

            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0f;

            fadeCanvasGroup.blocksRaycasts = true;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaledDeltaTime so fade works even if timescale is 0
                float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration); // SmoothStep for eased fade
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
            fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.01f;
        }
    }
}
