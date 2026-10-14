using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalFlush.Player;
using RoyalFlush.CameraSystem;

namespace RoyalFlush.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        
        [Header("Choice UI (Until Then Style)")]
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private Button leftChoiceButton;
        [SerializeField] private TextMeshProUGUI leftChoiceText;
        [SerializeField] private Button rightChoiceButton;
        [SerializeField] private TextMeshProUGUI rightChoiceText;

        [Header("Continue Indicator")]
        [SerializeField] private GameObject continueIndicator; // Small ▼ arrow shown after text finishes

        [Header("Audio Settings")]
        [SerializeField] private AudioSource typingAudioSource;
        [SerializeField] private AudioClip typewriterSound;
        [SerializeField] [Range(1, 5)] private int soundFrequency = 2;

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.04f;
        [SerializeField] private float fastTypingSpeed = 0.01f;
        [SerializeField] private float sentenceDelay = 0.15f; // Brief pause between sentences

        private Queue<string> sentences;
        private DialogueChoice[] currentChoices;
        private bool isTyping = false;
        private bool isWaitingForChoice = false;
        private string currentSentence = "";
        private Coroutine typingCoroutine;
        private System.Action onDialogueEndCallback;
        private Transform currentSpeakerTransform; // For camera focus
        private bool isFastForward = false;

        public bool IsInDialogue => dialoguePanel != null && dialoguePanel.activeInHierarchy;

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

            sentences = new Queue<string>();
            
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (choiceContainer != null)
                choiceContainer.SetActive(false);

            if (continueIndicator != null)
                continueIndicator.SetActive(false);

            // Hook UI Button clicks
            if (leftChoiceButton != null)
                leftChoiceButton.onClick.AddListener(() => OnChoiceSelected(0));

            if (rightChoiceButton != null)
                rightChoiceButton.onClick.AddListener(() => OnChoiceSelected(1));
        }

        /// <summary>
        /// Start a dialogue sequence. Optionally provide the speaker's transform for camera focus.
        /// </summary>
        public void StartDialogue(string characterName, string[] newSentences, DialogueChoice[] newChoices = null, System.Action onComplete = null, Transform speakerTransform = null)
        {
            if (dialoguePanel == null) return;

            onDialogueEndCallback = onComplete;
            currentSpeakerTransform = speakerTransform;
            isFastForward = false;

            // Lock player movement during conversation
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.SetMovementEnabled(false);
            }

            // Camera: focus on speaker if provided
            CameraFollow2D cam = FindAnyObjectByType<CameraFollow2D>();
            if (cam != null && speakerTransform != null)
            {
                cam.FocusOn(speakerTransform);
            }

            dialoguePanel.SetActive(true);
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (continueIndicator != null) continueIndicator.SetActive(false);

            if (nameText != null)
            {
                nameText.text = characterName;
                nameText.gameObject.SetActive(!string.IsNullOrEmpty(characterName));
            }

            sentences.Clear();
            currentChoices = newChoices;
            isWaitingForChoice = false;

            if (newSentences != null)
            {
                foreach (string sentence in newSentences)
                {
                    sentences.Enqueue(sentence);
                }
            }

            DisplayNextSentence();
        }

        public void DisplayNextSentence()
        {
            if (isWaitingForChoice) return;

            if (continueIndicator != null) continueIndicator.SetActive(false);

            if (isTyping)
            {
                // Complete current sentence instantly instead of skipping entirely
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                if (dialogueText != null) dialogueText.text = currentSentence;
                isTyping = false;

                // Show continue indicator
                if (continueIndicator != null) continueIndicator.SetActive(true);

                // If choices are attached and this is the final sentence, show choices immediately
                if (sentences.Count == 0 && currentChoices != null && currentChoices.Length > 0)
                {
                    if (continueIndicator != null) continueIndicator.SetActive(false);
                    ShowChoices();
                }
                return;
            }

            if (sentences.Count == 0)
            {
                if (currentChoices != null && currentChoices.Length > 0)
                {
                    ShowChoices();
                }
                else
                {
                    EndDialogue();
                }
                return;
            }

            currentSentence = sentences.Dequeue();
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
        }

        private IEnumerator TypeSentence(string sentence)
        {
            isTyping = true;
            isFastForward = false;
            if (dialogueText != null) dialogueText.text = "";

            int charCount = 0;
            foreach (char letter in sentence)
            {
                if (dialogueText != null) dialogueText.text += letter;
                charCount++;

                // Play typewriter sound
                if (typingAudioSource != null && typewriterSound != null && charCount % soundFrequency == 0 && !char.IsWhiteSpace(letter))
                {
                    typingAudioSource.pitch = Random.Range(0.95f, 1.05f); // Slight pitch variation
                    typingAudioSource.PlayOneShot(typewriterSound);
                }

                float speed = isFastForward ? fastTypingSpeed : typingSpeed;
                yield return new WaitForSeconds(speed);
            }

            isTyping = false;

            // Show continue indicator after typing finishes
            if (continueIndicator != null)
            {
                // Don't show continue indicator if choices are about to appear
                if (!(sentences.Count == 0 && currentChoices != null && currentChoices.Length > 0))
                {
                    continueIndicator.SetActive(true);
                }
            }

            // Check if we need to show choices after finishing typing the last sentence
            if (sentences.Count == 0 && currentChoices != null && currentChoices.Length > 0)
            {
                yield return new WaitForSeconds(sentenceDelay);
                ShowChoices();
            }
        }

        private void ShowChoices()
        {
            isWaitingForChoice = true;
            if (continueIndicator != null) continueIndicator.SetActive(false);

            if (choiceContainer != null)
            {
                choiceContainer.SetActive(true);
            }

            // Left Choice
            if (currentChoices.Length > 0 && leftChoiceButton != null)
            {
                leftChoiceButton.gameObject.SetActive(true);
                if (leftChoiceText != null)
                {
                    string prompt = !string.IsNullOrEmpty(currentChoices[0].buttonPrompt) ? currentChoices[0].buttonPrompt + " " : "(X) ";
                    leftChoiceText.text = $"{prompt}{currentChoices[0].choiceText}";
                }
            }
            else if (leftChoiceButton != null)
            {
                leftChoiceButton.gameObject.SetActive(false);
            }

            // Right Choice
            if (currentChoices.Length > 1 && rightChoiceButton != null)
            {
                rightChoiceButton.gameObject.SetActive(true);
                if (rightChoiceText != null)
                {
                    string prompt = !string.IsNullOrEmpty(currentChoices[1].buttonPrompt) ? currentChoices[1].buttonPrompt + " " : "(B) ";
                    rightChoiceText.text = $"{prompt}{currentChoices[1].choiceText}";
                }
            }
            else if (rightChoiceButton != null)
            {
                rightChoiceButton.gameObject.SetActive(false);
            }
        }

        public void OnChoiceSelected(int choiceIndex)
        {
            if (!isWaitingForChoice || currentChoices == null || choiceIndex >= currentChoices.Length)
                return;

            DialogueChoice selectedChoice = currentChoices[choiceIndex];
            
            // Clean choice state
            isWaitingForChoice = false;
            currentChoices = null;
            if (choiceContainer != null) choiceContainer.SetActive(false);

            // Queue follow up sentences if any
            if (selectedChoice.responseSentences != null && selectedChoice.responseSentences.Length > 0)
            {
                sentences.Clear();
                foreach (string sentence in selectedChoice.responseSentences)
                {
                    sentences.Enqueue(sentence);
                }
                DisplayNextSentence();
            }
            else
            {
                EndDialogue();
            }
        }

        public void EndDialogue()
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            isTyping = false;
            isWaitingForChoice = false;
            currentChoices = null;
            isFastForward = false;

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (continueIndicator != null) continueIndicator.SetActive(false);

            // Camera: release focus
            CameraFollow2D cam = FindAnyObjectByType<CameraFollow2D>();
            if (cam != null)
            {
                cam.ReleaseFocus();
            }

            // Unlock player movement
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.SetMovementEnabled(true);
            }

            currentSpeakerTransform = null;

            // Invoke and clear completion callback
            var callback = onDialogueEndCallback;
            onDialogueEndCallback = null;
            callback?.Invoke();
        }

        private void Update()
        {
            if (!IsInDialogue) return;

            // Handle Choice Key Shortcuts
            if (isWaitingForChoice && currentChoices != null)
            {
                // Choice 0 (Left - default X or custom)
                if (currentChoices.Length > 0)
                {
                    KeyCode leftKey = currentChoices[0].shortcutKey != KeyCode.None ? currentChoices[0].shortcutKey : KeyCode.X;
                    if (Input.GetKeyDown(leftKey) || Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        OnChoiceSelected(0);
                        return;
                    }
                }

                // Choice 1 (Right - default B or custom)
                if (currentChoices.Length > 1)
                {
                    KeyCode rightKey = currentChoices[1].shortcutKey != KeyCode.None ? currentChoices[1].shortcutKey : KeyCode.B;
                    if (Input.GetKeyDown(rightKey) || Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        OnChoiceSelected(1);
                        return;
                    }
                }

                return; // Do not advance normal dialogue while waiting for choice
            }

            // Hold Shift to fast-forward text
            if (isTyping)
            {
                isFastForward = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }

            // Press E or Space or Return to advance
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                DisplayNextSentence();
            }
        }
    }
}
