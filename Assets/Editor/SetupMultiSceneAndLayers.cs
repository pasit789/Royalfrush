using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalFlush.Dialogue;
using RoyalFlush.SceneManagement;
using RoyalFlush.Player;
using RoyalFlush.CameraSystem;
using RoyalFlush.Interaction;

namespace RoyalFlush.EditorTools
{
    public static class SetupMultiSceneAndLayers
    {
        [MenuItem("RoyalFlush/Setup Layers, Visible Floor, and Hallway Scene")]
        public static void SetupAll()
        {
            // First setup active scene (SampleScene - Classroom)
            SetupClassroomLayersAndExit();

            // Then create and setup the new scene (SchoolHallway)
            SetupHallwayScene();

            // Finally register both scenes in Build Settings
            RegisterBuildSettings();

            // Load back to Classroom so user starts from beginning
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

            Debug.Log("Multi-Scene Story and Layered Environment Setup Completed Successfully!");
        }

        private static void SetupClassroomLayersAndExit()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

            // 1. Get Sprites
            Sprite knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Sprite squareSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (squareSprite == null)
            {
                string[] guids = AssetDatabase.FindAssets("Square t:Sprite");
                if (guids.Length > 0)
                {
                    squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            GameObject classroomRoot = GameObject.Find("ClassroomEnvironment");
            if (classroomRoot == null) classroomRoot = new GameObject("ClassroomEnvironment");

            // 2. Visible Polished Wooden Floor (Midground Base)
            GameObject floorBar = GameObject.Find("VisibleWoodenFloor");
            if (floorBar == null)
            {
                floorBar = new GameObject("VisibleWoodenFloor", typeof(SpriteRenderer));
                floorBar.transform.SetParent(classroomRoot.transform, false);
            }
            floorBar.transform.position = new Vector3(0f, -1.95f, 0.5f);
            floorBar.transform.localScale = new Vector3(14f, 0.65f, 1f);
            SpriteRenderer floorSR = floorBar.GetComponent<SpriteRenderer>();
            floorSR.sprite = squareSprite;
            floorSR.color = new Color(0.22f, 0.14f, 0.09f, 0.95f); // Rich dark polished wood floor
            floorSR.sortingOrder = 0; // Behind characters, in front of background

            // Top highlight strip on floor
            GameObject floorRim = GameObject.Find("FloorHighlightRim");
            if (floorRim == null)
            {
                floorRim = new GameObject("FloorHighlightRim", typeof(SpriteRenderer));
                floorRim.transform.SetParent(floorBar.transform, false);
            }
            floorRim.transform.localPosition = new Vector3(0f, 0.46f, -0.05f);
            floorRim.transform.localScale = new Vector3(1f, 0.08f, 1f);
            SpriteRenderer rimSR = floorRim.GetComponent<SpriteRenderer>();
            rimSR.sprite = squareSprite;
            rimSR.color = new Color(0.55f, 0.38f, 0.22f, 0.8f); // Golden floorboard reflection
            rimSR.sortingOrder = 0;

            // 3. Foreground Layer (Desks in foreground creating 2.5D Parallax/Occlusion)
            GameObject foreRoot = GameObject.Find("ForegroundLayer");
            if (foreRoot == null)
            {
                foreRoot = new GameObject("ForegroundLayer");
                foreRoot.transform.SetParent(classroomRoot.transform, false);
            }

            // Foreground Left Desk
            CreateForegroundDesk(foreRoot.transform, "ForegroundDesk_Left", new Vector3(-3.8f, -2.3f, -1.5f), new Vector3(2.4f, 1.6f, 1f), squareSprite);
            // Foreground Right Desk
            CreateForegroundDesk(foreRoot.transform, "ForegroundDesk_Right", new Vector3(3.8f, -2.3f, -1.5f), new Vector3(2.4f, 1.6f, 1f), squareSprite);

            // Foreground Soft Golden Dust/Glow Overlay
            GameObject foreGlow = GameObject.Find("ForegroundGoldenGlow");
            if (foreGlow == null)
            {
                foreGlow = new GameObject("ForegroundGoldenGlow", typeof(SpriteRenderer));
                foreGlow.transform.SetParent(foreRoot.transform, false);
            }
            foreGlow.transform.position = new Vector3(0f, 0f, -2f);
            foreGlow.transform.localScale = new Vector3(14f, 8f, 1f);
            SpriteRenderer glowSR = foreGlow.GetComponent<SpriteRenderer>();
            glowSR.sprite = squareSprite;
            glowSR.color = new Color(1.0f, 0.72f, 0.35f, 0.06f); // Ultra soft cinematic golden bloom
            glowSR.sortingOrder = 12;

            // 4. Setup SceneTransitionManager & Fade UI
            SetupTransitionManager();

            // 5. Exit Trigger on Right Side (x = 5.2) to walk out of classroom
            GameObject exitTriggerGO = GameObject.Find("ExitTrigger_ToHallway");
            if (exitTriggerGO == null)
            {
                exitTriggerGO = new GameObject("ExitTrigger_ToHallway", typeof(BoxCollider2D), typeof(ExitTrigger2D));
            }
            exitTriggerGO.transform.position = new Vector3(5.0f, -0.9f, 0f);
            exitTriggerGO.transform.localScale = Vector3.one;

            BoxCollider2D exitCol = exitTriggerGO.GetComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            exitCol.size = new Vector2(1.2f, 2.5f);

            ExitTrigger2D exitScript = exitTriggerGO.GetComponent<ExitTrigger2D>();
            GameObject classmateGO = GameObject.Find("NPC_Classmate");
            if (classmateGO != null)
            {
                DialogueTrigger dt = classmateGO.GetComponent<DialogueTrigger>();
                SerializedObject so = new SerializedObject(exitScript);
                so.FindProperty("targetSceneName").stringValue = "SchoolHallway";
                so.FindProperty("requireDialogueComplete").boolValue = true;
                so.FindProperty("prerequisiteDialogue").objectReferenceValue = dt;
                so.FindProperty("lockedMessage").stringValue = "I shouldn't leave yet. I should talk to my classmate first.";
                so.ApplyModifiedProperties();
            }

            // Door Indicator visual on the right wall
            GameObject doorVisual = GameObject.Find("DoorIndicator");
            if (doorVisual == null)
            {
                doorVisual = new GameObject("DoorIndicator", typeof(SpriteRenderer));
                doorVisual.transform.SetParent(exitTriggerGO.transform, false);
            }
            doorVisual.transform.localPosition = new Vector3(0.3f, 0f, 0.1f);
            doorVisual.transform.localScale = new Vector3(0.3f, 2.3f, 1f);
            SpriteRenderer doorSR = doorVisual.GetComponent<SpriteRenderer>();
            doorSR.sprite = squareSprite;
            doorSR.color = new Color(0.9f, 0.7f, 0.3f, 0.4f); // Golden light leaking from hallway door
            doorSR.sortingOrder = 1;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Classroom Scene configured with layers, floor, and exit trigger!");
        }

        private static void CreateForegroundDesk(Transform parent, string name, Vector3 pos, Vector3 scale, Sprite sprite)
        {
            Transform existing = parent.Find(name);
            GameObject deskGO;
            if (existing == null)
            {
                deskGO = new GameObject(name, typeof(SpriteRenderer));
                deskGO.transform.SetParent(parent, false);
            }
            else
            {
                deskGO = existing.gameObject;
            }

            deskGO.transform.position = pos;
            deskGO.transform.localScale = scale;
            SpriteRenderer sr = deskGO.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.16f, 0.10f, 0.06f, 0.88f); // Dark silhouetted foreground desk
            sr.sortingOrder = 10; // In front of characters
        }

        private static void SetupTransitionManager()
        {
            SceneTransitionManager stm = Object.FindAnyObjectByType<SceneTransitionManager>();
            if (stm == null)
            {
                GameObject stmGO = new GameObject("SceneTransitionManager", typeof(SceneTransitionManager));
                
                // Create Fade Canvas
                GameObject fadeCanvasGO = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
                fadeCanvasGO.transform.SetParent(stmGO.transform, false);
                Canvas fc = fadeCanvasGO.GetComponent<Canvas>();
                fc.renderMode = RenderMode.ScreenSpaceOverlay;
                fc.sortingOrder = 999; // Topmost

                CanvasGroup cg = fadeCanvasGO.GetComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.blocksRaycasts = false;

                // Black Image
                GameObject blackOverlay = new GameObject("BlackOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                blackOverlay.transform.SetParent(fadeCanvasGO.transform, false);
                RectTransform rt = blackOverlay.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                Image img = blackOverlay.GetComponent<Image>();
                img.color = Color.black;

                SerializedObject so = new SerializedObject(stmGO.GetComponent<SceneTransitionManager>());
                so.FindProperty("fadeCanvasGroup").objectReferenceValue = cg;
                so.FindProperty("fadeDuration").floatValue = 0.5f;
                so.ApplyModifiedProperties();
            }
        }

        private static void SetupHallwayScene()
        {
            // Create or open SchoolHallway scene
            string hallwayPath = "Assets/Scenes/SchoolHallway.unity";
            UnityEngine.SceneManagement.Scene hallwayScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(hallwayScene, hallwayPath);

            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Square.png");
            if (squareSprite == null)
            {
                squareSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            }

            Sprite knobSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Capsule.png");
            if (knobSprite == null)
            {
                knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            }

            // 1. Main Camera
            GameObject camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow2D));
            Camera cam = camGO.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3.2f;
            cam.backgroundColor = new Color(0.14f, 0.08f, 0.06f, 1f); // Sunset evening tone
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(-4f, 0f, -10f);

            // 2. Hallway Environment Root
            GameObject hallwayRoot = new GameObject("HallwayEnvironment");

            // Hallway Back Wall (Warm Sunset Lit Corridor)
            GameObject backWall = new GameObject("HallwayBackWall", typeof(SpriteRenderer));
            backWall.transform.SetParent(hallwayRoot.transform, false);
            backWall.transform.position = new Vector3(0f, 0.5f, 5f);
            backWall.transform.localScale = new Vector3(20f, 6.5f, 1f);
            SpriteRenderer wallSR = backWall.GetComponent<SpriteRenderer>();
            wallSR.sprite = squareSprite;
            wallSR.color = new Color(0.78f, 0.65f, 0.50f, 1f); // Warm peach-cream corridor wall
            wallSR.sortingOrder = -10;

            // Corridor Windows (Sunset Sky & God Rays)
            for (int i = -2; i <= 2; i++)
            {
                GameObject win = new GameObject($"Window_{i}", typeof(SpriteRenderer));
                win.transform.SetParent(hallwayRoot.transform, false);
                win.transform.position = new Vector3(i * 4f, 1.0f, 4f);
                win.transform.localScale = new Vector3(2.5f, 3.2f, 1f);
                SpriteRenderer winSR = win.GetComponent<SpriteRenderer>();
                winSR.sprite = squareSprite;
                winSR.color = new Color(0.96f, 0.55f, 0.25f, 1f); // Vibrant amber sunset
                winSR.sortingOrder = -9;

                // Window Frame
                GameObject frame = new GameObject("Frame", typeof(SpriteRenderer));
                frame.transform.SetParent(win.transform, false);
                frame.transform.localPosition = Vector3.zero;
                frame.transform.localScale = new Vector3(1.08f, 1.08f, 1f);
                SpriteRenderer frameSR = frame.GetComponent<SpriteRenderer>();
                frameSR.sprite = squareSprite;
                frameSR.color = new Color(0.25f, 0.18f, 0.15f, 0.9f);
                frameSR.sortingOrder = -8;
            }

            // Hallway Floor (Visible Polished Wood Planks)
            GameObject floor = new GameObject("HallwayFloor", typeof(SpriteRenderer), typeof(BoxCollider2D));
            floor.name = "Ground";
            floor.transform.position = new Vector3(0f, -1.95f, 0f);
            floor.transform.localScale = new Vector3(22f, 0.7f, 1f);
            SpriteRenderer floorSR = floor.GetComponent<SpriteRenderer>();
            floorSR.sprite = squareSprite;
            floorSR.color = new Color(0.28f, 0.16f, 0.10f, 1f); // Deep wood
            floorSR.sortingOrder = 0;
            BoxCollider2D floorCol = floor.GetComponent<BoxCollider2D>();
            floorCol.size = Vector2.one;

            // 3. Player Character (Enters from Left Door)
            GameObject player = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(PlayerMovement), typeof(PlayerInteraction), typeof(Animator));
            player.transform.position = new Vector3(-5.5f, -0.9f, 0f);
            player.transform.localScale = new Vector3(0.55f, 1.15f, 1f);
            SpriteRenderer playerSR = player.GetComponent<SpriteRenderer>();
            playerSR.sprite = knobSprite;
            playerSR.color = new Color(0.4f, 0.65f, 0.55f, 1f); // Green uniform tint
            playerSR.sortingOrder = 2;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            CapsuleCollider2D capCol = player.GetComponent<CapsuleCollider2D>();
            capCol.size = new Vector2(1f, 2f);

            // Shadow under player
            GameObject pShadow = new GameObject("Shadow", typeof(SpriteRenderer));
            pShadow.transform.SetParent(player.transform, false);
            pShadow.transform.localPosition = new Vector3(0f, -0.5f, 0.1f);
            pShadow.transform.localScale = new Vector3(1.2f, 0.25f, 1f);
            SpriteRenderer pShadowSR = pShadow.GetComponent<SpriteRenderer>();
            pShadowSR.sprite = knobSprite;
            pShadowSR.color = new Color(0f, 0f, 0f, 0.35f);
            pShadowSR.sortingOrder = 1;

            // Connect Camera target
            CameraFollow2D camFollow = camGO.GetComponent<CameraFollow2D>();
            SerializedObject camObj = new SerializedObject(camFollow);
            camObj.FindProperty("target").objectReferenceValue = player.transform;
            camObj.FindProperty("offset").vector3Value = new Vector3(0f, 0.5f, -10f);
            camObj.FindProperty("smoothSpeed").floatValue = 0.15f;
            camObj.ApplyModifiedProperties();

            // 4. THE SKELETON GENTLEMAN (Standing by the corridor on the right)
            GameObject skeleton = new GameObject("NPC_SkeletonGentleman", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(DialogueTrigger));
            skeleton.transform.position = new Vector3(2.5f, -0.9f, 0f);
            skeleton.transform.localScale = new Vector3(0.55f, 1.25f, 1f);
            SpriteRenderer skelSR = skeleton.GetComponent<SpriteRenderer>();
            skelSR.sprite = knobSprite;
            skelSR.color = new Color(0.95f, 0.95f, 0.92f, 1f); // Bone white silhouette
            skelSR.sortingOrder = 2;

            // Skeleton Dark Blazer and Red Ascot
            GameObject blazer = new GameObject("DarkBlazer", typeof(SpriteRenderer));
            blazer.transform.SetParent(skeleton.transform, false);
            blazer.transform.localPosition = new Vector3(0f, -0.1f, -0.05f);
            blazer.transform.localScale = new Vector3(0.85f, 0.65f, 1f);
            SpriteRenderer blazerSR = blazer.GetComponent<SpriteRenderer>();
            blazerSR.sprite = squareSprite;
            blazerSR.color = new Color(0.12f, 0.12f, 0.16f, 1f); // Formal blazer
            blazerSR.sortingOrder = 3;

            GameObject ascot = new GameObject("RedAscot", typeof(SpriteRenderer));
            ascot.transform.SetParent(skeleton.transform, false);
            ascot.transform.localPosition = new Vector3(0f, 0.22f, -0.1f);
            ascot.transform.localScale = new Vector3(0.35f, 0.25f, 1f);
            SpriteRenderer ascotSR = ascot.GetComponent<SpriteRenderer>();
            ascotSR.sprite = squareSprite;
            ascotSR.color = new Color(0.85f, 0.15f, 0.18f, 1f); // Red ascot
            ascotSR.sortingOrder = 4;

            // Shadow under skeleton
            GameObject sShadow = new GameObject("Shadow", typeof(SpriteRenderer));
            sShadow.transform.SetParent(skeleton.transform, false);
            sShadow.transform.localPosition = new Vector3(0f, -0.5f, 0.1f);
            sShadow.transform.localScale = new Vector3(1.2f, 0.25f, 1f);
            SpriteRenderer sShadowSR = sShadow.GetComponent<SpriteRenderer>();
            sShadowSR.sprite = knobSprite;
            sShadowSR.color = new Color(0f, 0f, 0f, 0.35f);
            sShadowSR.sortingOrder = 1;

            BoxCollider2D skelCol = skeleton.GetComponent<BoxCollider2D>();
            skelCol.isTrigger = true;
            skelCol.size = new Vector2(2f, 2f);

            // Skeleton Dialogue & Choices
            DialogueTrigger skelTrigger = skeleton.GetComponent<DialogueTrigger>();
            skelTrigger.characterName = "Mr. Bones";
            skelTrigger.sentences = new string[]
            {
                "Ah... You finally made it out of the classroom.",
                "Did you say everything you wanted to say to her?"
            };
            skelTrigger.choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    buttonPrompt = "(X)",
                    choiceText = "How do you know about that?",
                    shortcutKey = KeyCode.X,
                    responseSentences = new string[]
                    {
                        "Heh heh... An old skeleton sees everything that echoes through these corridors.",
                        "Now hurry along. The golden hour won't linger forever."
                    }
                },
                new DialogueChoice
                {
                    buttonPrompt = "(B)",
                    choiceText = "It doesn't matter now. Let's keep moving.",
                    shortcutKey = KeyCode.B,
                    responseSentences = new string[]
                    {
                        "A stoic answer. I admire that.",
                        "Come along then. The evening has only just begun."
                    }
                }
            };

            // 5. Door on the Left (Back to Classroom)
            GameObject returnDoor = new GameObject("Door_ToClassroom", typeof(BoxCollider2D), typeof(DoorInteractable));
            returnDoor.transform.position = new Vector3(-6.5f, -0.9f, 0f);
            BoxCollider2D returnCol = returnDoor.GetComponent<BoxCollider2D>();
            returnCol.isTrigger = true;
            returnCol.size = new Vector2(1.5f, 2.5f);

            DoorInteractable di = returnDoor.GetComponent<DoorInteractable>();
            SerializedObject diSO = new SerializedObject(di);
            diSO.FindProperty("transitionType").enumValueIndex = (int)DoorInteractable.TransitionType.LoadNewScene;
            diSO.FindProperty("targetSceneName").stringValue = "SampleScene";
            diSO.ApplyModifiedProperties();

            // 6. Setup Canvas and DialogueManager in Hallway
            SetupHallwayCanvasAndDialogue();

            // 7. EventSystem
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

            EditorSceneManager.MarkSceneDirty(hallwayScene);
            EditorSceneManager.SaveScene(hallwayScene);
            Debug.Log("SchoolHallway Scene created and populated successfully!");
        }

        private static void SetupHallwayCanvasAndDialogue()
        {
            // Canvas
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Dialogue Panel
            GameObject panelGO = new GameObject("DialoguePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 34f);
            panelRect.sizeDelta = new Vector2(800f, 150f);
            Image panelImg = panelGO.GetComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.10f, 0.85f);

            // NameText
            GameObject nameGO = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(panelGO.transform, false);
            RectTransform nameRect = nameGO.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(-280f, 45f);
            nameRect.sizeDelta = new Vector2(200f, 40f);
            TextMeshProUGUI nameTMP = nameGO.GetComponent<TextMeshProUGUI>();
            nameTMP.fontSize = 24f;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.color = new Color(0.95f, 0.75f, 0.35f, 1f); // Warm gold character name

            // DialogueText
            GameObject diagGO = new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
            diagGO.transform.SetParent(panelGO.transform, false);
            RectTransform diagRect = diagGO.GetComponent<RectTransform>();
            diagRect.anchoredPosition = new Vector2(0f, -10f);
            diagRect.sizeDelta = new Vector2(740f, 80f);
            TextMeshProUGUI diagTMP = diagGO.GetComponent<TextMeshProUGUI>();
            diagTMP.fontSize = 20f;
            diagTMP.color = Color.white;

            // ChoiceContainer
            GameObject choiceContainerGO = new GameObject("ChoiceContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            choiceContainerGO.transform.SetParent(canvasGO.transform, false);
            choiceContainerGO.layer = LayerMask.NameToLayer("UI");
            RectTransform contRect = choiceContainerGO.GetComponent<RectTransform>();
            contRect.anchorMin = new Vector2(0.5f, 0f);
            contRect.anchorMax = new Vector2(0.5f, 0f);
            contRect.pivot = new Vector2(0.5f, 0f);
            contRect.anchoredPosition = new Vector2(0f, 205f);
            contRect.sizeDelta = new Vector2(850f, 60f);

            HorizontalLayoutGroup layout = choiceContainerGO.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 30f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            Button leftBtn = SetupDialogueChoiceUI_Helper(choiceContainerGO.transform, "LeftChoiceButton", "(X) How do you know about that?");
            Button rightBtn = SetupDialogueChoiceUI_Helper(choiceContainerGO.transform, "RightChoiceButton", "(B) It doesn't matter now.");

            // GameManager with DialogueManager
            GameObject gmGO = new GameObject("GameManager", typeof(DialogueManager));
            DialogueManager dm = gmGO.GetComponent<DialogueManager>();
            SerializedObject dmSO = new SerializedObject(dm);
            dmSO.FindProperty("dialoguePanel").objectReferenceValue = panelGO;
            dmSO.FindProperty("nameText").objectReferenceValue = nameTMP;
            dmSO.FindProperty("dialogueText").objectReferenceValue = diagTMP;
            dmSO.FindProperty("choiceContainer").objectReferenceValue = choiceContainerGO;
            dmSO.FindProperty("leftChoiceButton").objectReferenceValue = leftBtn;
            dmSO.FindProperty("leftChoiceText").objectReferenceValue = leftBtn.GetComponentInChildren<TextMeshProUGUI>();
            dmSO.FindProperty("rightChoiceButton").objectReferenceValue = rightBtn;
            dmSO.FindProperty("rightChoiceText").objectReferenceValue = rightBtn.GetComponentInChildren<TextMeshProUGUI>();
            dmSO.ApplyModifiedProperties();

            panelGO.SetActive(false);
            choiceContainerGO.SetActive(false);

            // SceneTransitionManager
            SetupTransitionManager();
        }

        private static Button SetupDialogueChoiceUI_Helper(Transform parent, string name, string text)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            btnGO.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 50f);

            Image img = btnGO.GetComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.15f, 0.92f);

            Button btn = btnGO.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.12f, 0.12f, 0.15f, 0.92f);
            cb.highlightedColor = new Color(0.25f, 0.35f, 0.5f, 0.98f);
            cb.pressedColor = new Color(0.15f, 0.22f, 0.35f, 1f);
            cb.selectedColor = new Color(0.25f, 0.35f, 0.5f, 0.98f);
            btn.colors = cb;

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(btnGO.transform, false);
            textGO.layer = LayerMask.NameToLayer("UI");
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }

        private static void RegisterBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/SchoolHallway.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("Build Settings updated with SampleScene and SchoolHallway!");
        }
    }
}
