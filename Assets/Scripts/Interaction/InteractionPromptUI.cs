using UnityEngine;
using TMPro;

namespace RoyalFlush.Interaction
{
    /// <summary>
    /// Floating "Press E" prompt that appears above interactable objects.
    /// Attach to a world-space Canvas or TextMeshPro above the NPC/object.
    /// This script handles showing/hiding and a gentle bobbing animation.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("Prompt Settings")]
        [SerializeField] private string promptText = "Press E";
        [SerializeField] private TextMeshProUGUI promptLabel;
        [SerializeField] private TextMeshPro promptLabel3D; // Alternative: world-space TMP
        [SerializeField] private GameObject promptRoot;

        [Header("Animation")]
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float fadeSpeed = 5f;

        private bool isVisible = false;
        private float currentAlpha = 0f;
        private Vector3 baseLocalPosition;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (promptRoot != null)
            {
                baseLocalPosition = promptRoot.transform.localPosition;
                canvasGroup = promptRoot.GetComponent<CanvasGroup>();
            }

            SetText(promptText);
            SetAlpha(0f);
        }

        private void Update()
        {
            // Smooth fade in/out
            float targetAlpha = isVisible ? 1f : 0f;
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            SetAlpha(currentAlpha);

            // Bobbing animation
            if (promptRoot != null && currentAlpha > 0.01f)
            {
                float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
                promptRoot.transform.localPosition = baseLocalPosition + new Vector3(0f, bob, 0f);
            }

            // Hide if alpha is zero
            if (promptRoot != null)
            {
                promptRoot.SetActive(currentAlpha > 0.01f);
            }
        }

        public void Show()
        {
            isVisible = true;
            if (promptRoot != null) promptRoot.SetActive(true);
        }

        public void Hide()
        {
            isVisible = false;
        }

        public void SetText(string text)
        {
            if (promptLabel != null) promptLabel.text = text;
            if (promptLabel3D != null) promptLabel3D.text = text;
        }

        private void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            else
            {
                // Fallback: set color alpha on TMP
                if (promptLabel != null)
                {
                    Color c = promptLabel.color;
                    c.a = alpha;
                    promptLabel.color = c;
                }
                if (promptLabel3D != null)
                {
                    Color c = promptLabel3D.color;
                    c.a = alpha;
                    promptLabel3D.color = c;
                }
            }
        }
    }
}
