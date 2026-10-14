using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalFlush.Dialogue;

namespace RoyalFlush.EditorTools
{
    public static class SetupDialogueChoiceUI
    {
        [MenuItem("RoyalFlush/Setup Choice UI and Sample Dialogue")]
        public static void SetupUI()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in the scene!");
                return;
            }

            // Find or create DialoguePanel
            Transform dialoguePanelTr = canvas.transform.Find("DialoguePanel");
            if (dialoguePanelTr == null)
            {
                Debug.LogError("DialoguePanel not found under Canvas!");
                return;
            }

            // 1. Style DialoguePanel to be sleek Until-Then aesthetic
            Image panelImage = dialoguePanelTr.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.08f, 0.08f, 0.10f, 0.85f); // Deep dark sleek panel
            }

            // 2. Setup ChoiceContainer above DialoguePanel
            Transform choiceContainerTr = canvas.transform.Find("ChoiceContainer");
            GameObject choiceContainerGO;
            if (choiceContainerTr == null)
            {
                choiceContainerGO = new GameObject("ChoiceContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                choiceContainerGO.transform.SetParent(canvas.transform, false);
            }
            else
            {
                choiceContainerGO = choiceContainerTr.gameObject;
            }

            int uiLayer = LayerMask.NameToLayer("UI");
            choiceContainerGO.layer = uiLayer;

            RectTransform containerRect = choiceContainerGO.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0f);
            containerRect.anchorMax = new Vector2(0.5f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, 205f); // Floats directly above dialogue box
            containerRect.sizeDelta = new Vector2(850f, 60f);

            HorizontalLayoutGroup layout = choiceContainerGO.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 30f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // 3. Create Left and Right Choice Buttons
            Button leftButton = CreateOrGetChoiceButton(choiceContainerGO.transform, "LeftChoiceButton", new Vector2(400f, 50f), "(X) You just came here to flirt?");
            Button rightButton = CreateOrGetChoiceButton(choiceContainerGO.transform, "RightChoiceButton", new Vector2(400f, 50f), "(B) I support you");

            // 4. Wire everything to DialogueManager
            DialogueManager dm = Object.FindAnyObjectByType<DialogueManager>();
            if (dm != null)
            {
                SerializedObject serializedDM = new SerializedObject(dm);
                
                SerializedProperty choiceContainerProp = serializedDM.FindProperty("choiceContainer");
                SerializedProperty leftBtnProp = serializedDM.FindProperty("leftChoiceButton");
                SerializedProperty leftTextProp = serializedDM.FindProperty("leftChoiceText");
                SerializedProperty rightBtnProp = serializedDM.FindProperty("rightChoiceButton");
                SerializedProperty rightTextProp = serializedDM.FindProperty("rightChoiceText");

                if (choiceContainerProp != null) choiceContainerProp.objectReferenceValue = choiceContainerGO;
                if (leftBtnProp != null) leftBtnProp.objectReferenceValue = leftButton;
                if (leftTextProp != null) leftTextProp.objectReferenceValue = leftButton.GetComponentInChildren<TextMeshProUGUI>();
                if (rightBtnProp != null) rightBtnProp.objectReferenceValue = rightButton;
                if (rightTextProp != null) rightTextProp.objectReferenceValue = rightButton.GetComponentInChildren<TextMeshProUGUI>();

                serializedDM.ApplyModifiedProperties();
                Debug.Log("DialogueManager properties connected successfully!");
            }

            // 5. Setup English Dialogue on Classmate NPC
            GameObject box = GameObject.Find("NPC_Classmate");
            if (box == null) box = GameObject.Find("InteractableBox");

            if (box != null)
            {
                DialogueTrigger trigger = box.GetComponent<DialogueTrigger>();
                if (trigger != null)
                {
                    trigger.characterName = "Classmate";
                    trigger.sentences = new string[]
                    {
                        "Hey... What have you been staring at since class started?",
                        "Did you have something you wanted to say to me?"
                    };

                    trigger.choices = new DialogueChoice[]
                    {
                        new DialogueChoice
                        {
                            buttonPrompt = "(X)",
                            choiceText = "You just came here to flirt?",
                            shortcutKey = KeyCode.X,
                            responseSentences = new string[]
                            {
                                "Are you serious?! Who would ever come here to flirt with you?!",
                                "Stop being so full of yourself... Just hurry up and pack your bag, school is closing soon."
                            }
                        },
                        new DialogueChoice
                        {
                            buttonPrompt = "(B)",
                            choiceText = "I support you.",
                            shortcutKey = KeyCode.B,
                            responseSentences = new string[]
                            {
                                "Thanks...",
                                "Honestly, I've had so much on my mind lately. It really means a lot hearing that from you."
                            }
                        }
                    };

                    EditorUtility.SetDirty(trigger);
                }
            }

            // 6. Setup Camera Follow on Main Camera
            Camera mainCam = Camera.main;
            GameObject playerObj = GameObject.Find("Player");
            if (mainCam != null && playerObj != null)
            {
                RoyalFlush.CameraSystem.CameraFollow2D camFollow = mainCam.GetComponent<RoyalFlush.CameraSystem.CameraFollow2D>();
                if (camFollow == null)
                {
                    camFollow = mainCam.gameObject.AddComponent<RoyalFlush.CameraSystem.CameraFollow2D>();
                }

                SerializedObject camObj = new SerializedObject(camFollow);
                SerializedProperty targetProp = camObj.FindProperty("target");
                SerializedProperty offsetProp = camObj.FindProperty("offset");
                SerializedProperty smoothSpeedProp = camObj.FindProperty("smoothSpeed");

                if (targetProp != null) targetProp.objectReferenceValue = playerObj.transform;
                if (offsetProp != null) offsetProp.vector3Value = new Vector3(0f, 0.4f, -10f);
                if (smoothSpeedProp != null) smoothSpeedProp.floatValue = 0.15f;

                camObj.ApplyModifiedProperties();
                EditorUtility.SetDirty(mainCam);
                Debug.Log("CameraFollow2D attached and configured on Main Camera!");
            }

            // Deactivate choice container initially in editor
            choiceContainerGO.SetActive(false);

            // Mark active scene dirty & save
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("Royal Flush Dialogue Choice UI and Camera Follow Setup Complete!");
        }

        private static Button CreateOrGetChoiceButton(Transform parent, string name, Vector2 size, string defaultText)
        {
            Transform existing = parent.Find(name);
            GameObject btnGO;
            if (existing == null)
            {
                btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnGO.transform.SetParent(parent, false);
            }
            else
            {
                btnGO = existing.gameObject;
            }

            btnGO.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            // Sleek styling: semi-transparent dark with a subtle border or tint
            Image img = btnGO.GetComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.15f, 0.92f);

            Button btn = btnGO.GetComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.12f, 0.12f, 0.15f, 0.92f);
            colors.highlightedColor = new Color(0.25f, 0.35f, 0.5f, 0.98f);
            colors.pressedColor = new Color(0.15f, 0.22f, 0.35f, 1f);
            colors.selectedColor = new Color(0.25f, 0.35f, 0.5f, 0.98f);
            btn.colors = colors;

            // Setup Text child
            Transform textTr = btnGO.transform.Find("Text");
            GameObject textGO;
            if (textTr == null)
            {
                textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textGO.transform.SetParent(btnGO.transform, false);
            }
            else
            {
                textGO = textTr.gameObject;
            }

            textGO.layer = LayerMask.NameToLayer("UI");
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = 18f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }
    }
}
