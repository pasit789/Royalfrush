using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoyalFlush.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        
        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.05f;

        private Queue<string> sentences;
        private bool isTyping = false;
        private string currentSentence = "";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            sentences = new Queue<string>();
            dialoguePanel.SetActive(false);
        }

        public void StartDialogue(string characterName, string[] newSentences)
        {
            dialoguePanel.SetActive(true);
            nameText.text = characterName;
            sentences.Clear();

            foreach (string sentence in newSentences)
            {
                sentences.Enqueue(sentence);
            }

            DisplayNextSentence();
        }

        public void DisplayNextSentence()
        {
            if (isTyping)
            {
                // If the player clicks/presses next while typing, complete the sentence instantly
                StopAllCoroutines();
                dialogueText.text = currentSentence;
                isTyping = false;
                return;
            }

            if (sentences.Count == 0)
            {
                EndDialogue();
                return;
            }

            currentSentence = sentences.Dequeue();
            StartCoroutine(TypeSentence(currentSentence));
        }

        private IEnumerator TypeSentence(string sentence)
        {
            isTyping = true;
            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            isTyping = false;
        }

        private void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            // Optionally tell the player controller to enable movement again here
        }

        private void Update()
        {
            // Press E or Space to go to the next sentence if the panel is active
            if (dialoguePanel.activeInHierarchy && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
            {
                DisplayNextSentence();
            }
        }
    }
}
