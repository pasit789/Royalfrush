using System;
using UnityEngine;

namespace RoyalFlush.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        [Tooltip("Button prompt indicator, e.g. (X) or (B)")]
        public string buttonPrompt = "(X)";
        
        [Tooltip("Text shown on the choice button")]
        [TextArea(1, 3)]
        public string choiceText;

        [Tooltip("Keyboard shortcut for this choice")]
        public KeyCode shortcutKey = KeyCode.X;

        [Tooltip("Dialogue lines that will follow after selecting this choice")]
        [TextArea(2, 5)]
        public string[] responseSentences;
    }
}
