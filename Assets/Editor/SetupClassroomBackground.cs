using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RoyalFlush.EditorTools
{
    public static class SetupClassroomBackground
    {
        [MenuItem("RoyalFlush/Apply Classroom Background Image")]
        public static void ApplyBackground()
        {
            string imagePath = "Assets/image/classroombackground.png";

            // 1. Ensure texture is imported as 2D Sprite
            TextureImporter importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (importer != null)
            {
                bool needReimport = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    needReimport = true;
                }
                if (importer.spritePixelsPerUnit != 100f)
                {
                    importer.spritePixelsPerUnit = 100f;
                    needReimport = true;
                }
                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    needReimport = true;
                }

                if (needReimport)
                {
                    importer.SaveAndReimport();
                    Debug.Log("classroombackground.png reimported as 2D Sprite!");
                }
            }
            else
            {
                Debug.LogError("Could not find TextureImporter for " + imagePath);
                return;
            }

            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            if (bgSprite == null)
            {
                Debug.LogError("Failed to load Sprite at " + imagePath);
                return;
            }

            // 2. Setup ClassroomEnvironment Parent
            GameObject classroomRoot = GameObject.Find("ClassroomEnvironment");
            if (classroomRoot == null)
            {
                classroomRoot = new GameObject("ClassroomEnvironment");
            }

            // Clean up old geometric placeholders inside ClassroomEnvironment
            for (int i = classroomRoot.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(classroomRoot.transform.GetChild(i).gameObject);
            }

            // 3. Create Main Background Sprite (Scaled to fill 16:9 screen)
            GameObject bgGO = new GameObject("ClassroomBackgroundSprite", typeof(SpriteRenderer));
            bgGO.transform.SetParent(classroomRoot.transform, false);
            bgGO.transform.position = new Vector3(0f, 0.15f, 2f);
            bgGO.transform.localScale = new Vector3(1.35f, 1.35f, 1f); // Fills 16:9 widescreen smoothly

            SpriteRenderer bgSR = bgGO.GetComponent<SpriteRenderer>();
            bgSR.sprite = bgSprite;
            bgSR.sortingOrder = -10; // Furthest back

            // 4. Position Ground collider so characters walk on the classroom floor
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.transform.position = new Vector3(0f, -1.65f, 0f);
                ground.transform.localScale = new Vector3(15f, 0.3f, 1f);
                
                SpriteRenderer sr = ground.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = false;
                }

                BoxCollider2D col = ground.GetComponent<BoxCollider2D>();
                if (col == null)
                {
                    col = ground.AddComponent<BoxCollider2D>();
                }
            }

            // 5. Place Player and Classmate NPC with realistic proportions
            Sprite capsuleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(-0.8f, -0.9f, 0f);
                player.transform.localScale = new Vector3(0.55f, 1.15f, 1f);
                SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 2;
                }

                // Add subtle shadow under player
                CreateOrUpdateShadow(player.transform, capsuleSprite);
            }

            GameObject npc = GameObject.Find("NPC_Classmate");
            if (npc == null) npc = GameObject.Find("InteractableBox");
            if (npc != null)
            {
                npc.name = "NPC_Classmate";
                npc.transform.position = new Vector3(1.0f, -0.9f, 0f);
                npc.transform.localScale = new Vector3(0.55f, 1.15f, 1f);
                SpriteRenderer sr = npc.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 2;
                }

                // Add subtle shadow under NPC
                CreateOrUpdateShadow(npc.transform, capsuleSprite);
            }

            // 6. Adjust Camera position and bounds
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 0f, -10f);
                cam.orthographic = true;
                cam.orthographicSize = 3.2f;
                cam.backgroundColor = new Color(0.12f, 0.08f, 0.06f, 1f);

                // Update CameraFollow2D offset
                RoyalFlush.CameraSystem.CameraFollow2D camFollow = cam.GetComponent<RoyalFlush.CameraSystem.CameraFollow2D>();
                if (camFollow != null)
                {
                    SerializedObject camObj = new SerializedObject(camFollow);
                    SerializedProperty offsetProp = camObj.FindProperty("offset");
                    if (offsetProp != null) offsetProp.vector3Value = new Vector3(0f, 0.5f, -10f);
                    camObj.ApplyModifiedProperties();
                }
            }

            // 7. Save Scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("Classroom background and environment successfully updated!");
        }

        private static void CreateOrUpdateShadow(Transform parent, Sprite shadowSprite)
        {
            Transform existing = parent.Find("Shadow");
            GameObject shadowGO;
            if (existing == null)
            {
                shadowGO = new GameObject("Shadow", typeof(SpriteRenderer));
                shadowGO.transform.SetParent(parent, false);
            }
            else
            {
                shadowGO = existing.gameObject;
            }

            shadowGO.transform.localPosition = new Vector3(0f, -0.5f, 0.1f);
            shadowGO.transform.localScale = new Vector3(1.2f, 0.25f, 1f);

            SpriteRenderer sr = shadowGO.GetComponent<SpriteRenderer>();
            if (shadowSprite != null) sr.sprite = shadowSprite;
            sr.color = new Color(0f, 0f, 0f, 0.35f); // Soft ambient occlusion shadow
            sr.sortingOrder = 1; // Under the character
        }
    }
}
