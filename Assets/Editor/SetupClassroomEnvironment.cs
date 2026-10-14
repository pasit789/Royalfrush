using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RoyalFlush.EditorTools
{
    public static class SetupClassroomEnvironment
    {
        [MenuItem("RoyalFlush/Setup Classroom Environment Scene")]
        public static void SetupClassroom()
        {
            // 1. Adjust Camera for Cinematic 2.5D framing
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(1.5f, -1.8f, -10f);
                cam.orthographic = true;
                cam.orthographicSize = 3.6f;
                // Warm sunset twilight background
                cam.backgroundColor = new Color(0.18f, 0.12f, 0.16f, 1f);
            }

            // 2. Classroom Root
            GameObject classroomRoot = GameObject.Find("ClassroomEnvironment");
            if (classroomRoot == null)
            {
                classroomRoot = new GameObject("ClassroomEnvironment");
            }

            // Helper to get square sprite from Unity
            Sprite squareSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (squareSprite == null)
            {
                // Fallback to searching Unity sprite
                string[] guids = AssetDatabase.FindAssets("Square t:Sprite");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
            }

            // Back Wall (Vintage Yellowish-Cream School Wall)
            CreateOrUpdateLayer(classroomRoot.transform, "BackWall", squareSprite, 
                new Vector3(1.5f, -0.5f, 5f), new Vector3(18f, 7f, 1f), 
                new Color(0.85f, 0.80f, 0.68f, 1f), -10);

            // Floor (Warm Dark Wooden School Floor)
            CreateOrUpdateLayer(classroomRoot.transform, "WoodenFloor", squareSprite, 
                new Vector3(1.5f, -3.8f, 4f), new Vector3(18f, 2f, 1f), 
                new Color(0.35f, 0.22f, 0.14f, 1f), -9);

            // Large Windows (Golden Hour Sky outside)
            CreateOrUpdateLayer(classroomRoot.transform, "WindowSky", squareSprite, 
                new Vector3(-2.5f, 0.2f, 4.5f), new Vector3(4.5f, 3.2f, 1f), 
                new Color(0.96f, 0.62f, 0.32f, 1f), -8);

            // Window Bar Frames (Iron bars / dividers)
            CreateOrUpdateLayer(classroomRoot.transform, "WindowFrame", squareSprite, 
                new Vector3(-2.5f, 0.2f, 4.4f), new Vector3(4.7f, 3.4f, 1f), 
                new Color(0.20f, 0.18f, 0.18f, 0.85f), -7);

            // Window Glass Overlay / Golden Glow
            CreateOrUpdateLayer(classroomRoot.transform, "GoldenSunbeam", squareSprite, 
                new Vector3(-0.5f, -1.2f, 2f), new Vector3(8f, 4.5f, 1f), 
                new Color(1.0f, 0.82f, 0.45f, 0.16f), 2); // Foreground sunbeam bloom

            // Chalkboard on the Right
            CreateOrUpdateLayer(classroomRoot.transform, "Chalkboard", squareSprite, 
                new Vector3(6.5f, -0.6f, 4f), new Vector3(4.8f, 2.5f, 1f), 
                new Color(0.12f, 0.24f, 0.16f, 1f), -8);

            // Chalkboard Wood Border
            CreateOrUpdateLayer(classroomRoot.transform, "ChalkboardFrame", squareSprite, 
                new Vector3(6.5f, -0.6f, 4.1f), new Vector3(5.0f, 2.7f, 1f), 
                new Color(0.42f, 0.28f, 0.18f, 1f), -9);

            // Student Desks (Wood & Dark Metal Legs)
            CreateOrUpdateLayer(classroomRoot.transform, "StudentDesk_Background", squareSprite, 
                new Vector3(-0.5f, -2.4f, 1f), new Vector3(1.8f, 1.2f, 1f), 
                new Color(0.45f, 0.30f, 0.18f, 1f), -2);

            CreateOrUpdateLayer(classroomRoot.transform, "StudentDesk_Foreground", squareSprite, 
                new Vector3(2.6f, -2.6f, -1f), new Vector3(2.0f, 1.3f, 1f), 
                new Color(0.40f, 0.26f, 0.15f, 1f), 1);

            // 3. Update Existing Ground to match Wooden Floor
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.transform.position = new Vector3(1.5f, -4.0f, 0f);
                ground.transform.localScale = new Vector3(18f, 0.5f, 1f);
                SpriteRenderer sr = ground.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(0.25f, 0.16f, 0.10f, 1f);
                }
            }

            // 4. Update InteractableBox to represent Classmate / NPC
            GameObject box = GameObject.Find("InteractableBox");
            if (box != null)
            {
                box.name = "NPC_Classmate";
                box.transform.position = new Vector3(2.6f, -2.2f, 0f);
                box.transform.localScale = new Vector3(0.85f, 1.6f, 1f);
                SpriteRenderer sr = box.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Warm student uniform color
                    sr.color = new Color(0.92f, 0.75f, 0.65f, 1f);
                }
            }

            // 5. Update Player visuals / positioning
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0.5f, -2.2f, 0f);
                player.transform.localScale = new Vector3(0.85f, 1.6f, 1f);
                SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Distinct character tint (Thai boy uniform vibe)
                    sr.color = new Color(0.4f, 0.65f, 0.55f, 1f); // Emerald green uniform accent
                }
            }

            // Save Scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("Classroom Environment created successfully!");
        }

        private static GameObject CreateOrUpdateLayer(Transform parent, string name, Sprite sprite, Vector3 pos, Vector3 scale, Color color, int orderInLayer)
        {
            Transform existing = parent.Find(name);
            GameObject go;
            if (existing == null)
            {
                go = new GameObject(name, typeof(SpriteRenderer));
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = existing.gameObject;
            }

            go.transform.position = pos;
            go.transform.localScale = scale;

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sprite != null) sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = orderInLayer;

            return go;
        }
    }
}
