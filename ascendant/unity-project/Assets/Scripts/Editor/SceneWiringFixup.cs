using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace Ascendant.EditorTools
{
    public static class SceneWiringFixup
    {
        static TMP_FontAsset _cachedFont;

        [MenuItem("Ascendant/Fix All Visual Issues")]
        public static void FixAllVisualIssues()
        {
            Debug.Log("=== SceneWiringFixup: Starting comprehensive fix ===");

            // Ensure we have a TMP font before doing anything
            EnsureTMPFont();

            FixDamageNumberPrefab();
            FixEnemyPrefab();
            FixScene();

            AssetDatabase.SaveAssets();
            Debug.Log("=== SceneWiringFixup: All fixes complete ===");
        }

        static void EnsureTMPFont()
        {
            _cachedFont = FindOrCreateFont();
            if (_cachedFont == null)
                Debug.LogError("[Fixup] CRITICAL: Could not find or create TMP font!");
            else
                Debug.Log($"[Fixup] Using font: {_cachedFont.name}");
        }

        static TMP_FontAsset FindOrCreateFont()
        {
            // 1. Try TMP_Settings (may throw in batch mode)
            try
            {
                var settings = TMP_Settings.defaultFontAsset;
                if (settings != null) return settings;
            }
            catch { /* Expected in batch mode */ }

            // 2. Search project for any existing TMP_FontAsset
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font != null)
                {
                    Debug.Log($"[Fixup] Found existing TMP font: {font.name} at {path}");
                    return font;
                }
            }

            // 3. Import TMP Essential Resources from the ugui package
            Debug.Log("[Fixup] No TMP font found. Importing TMP Essential Resources...");
            ImportTMPEssentialResources();

            // 4. Search again after import
            guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font != null)
                {
                    Debug.Log($"[Fixup] Found TMP font after import: {font.name} at {path}");
                    return font;
                }
            }

            // 5. Try TMP_Settings again after import
            try
            {
                var settings = TMP_Settings.defaultFontAsset;
                if (settings != null) return settings;
            }
            catch { }

            Debug.LogError("[Fixup] Could not find TMP font even after importing Essential Resources!");
            return null;
        }

        static void ImportTMPEssentialResources()
        {
            // Find the unitypackage in the ugui package
            string packageCachePath = "Library/PackageCache";
            string[] dirs = Directory.GetDirectories(packageCachePath, "com.unity.ugui@*");
            string packagePath = null;

            foreach (string dir in dirs)
            {
                string candidate = Path.Combine(dir, "Package Resources", "TMP Essential Resources.unitypackage");
                if (File.Exists(candidate))
                {
                    packagePath = candidate;
                    break;
                }
            }

            if (packagePath == null)
            {
                Debug.LogWarning("[Fixup] TMP Essential Resources.unitypackage not found in ugui package");
                return;
            }

            Debug.Log($"[Fixup] Importing from: {packagePath}");
            AssetDatabase.ImportPackage(packagePath, false); // false = no dialog
            AssetDatabase.Refresh();
            Debug.Log("[Fixup] TMP Essential Resources imported");
        }

        static void FixDamageNumberPrefab()
        {
            string prefabPath = "Assets/Prefabs/UI/DamageNumberPrefab.prefab";
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogWarning("[Fixup] DamageNumber prefab not found at " + prefabPath);
                return;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            var tmp = prefabRoot.GetComponent<TextMeshPro>();

            if (tmp != null && _cachedFont != null)
            {
                // Assign font
                var so = new SerializedObject(tmp);
                var fontProp = so.FindProperty("m_fontAsset");
                if (fontProp != null)
                    fontProp.objectReferenceValue = _cachedFont;

                // Assign shared material from font
                var matProp = so.FindProperty("m_sharedMaterial");
                if (matProp != null && _cachedFont.material != null)
                    matProp.objectReferenceValue = _cachedFont.material;

                so.ApplyModifiedPropertiesWithoutUndo();

                // Ensure isOrthographic for 2D game
                tmp.isOrthographic = true;
                EditorUtility.SetDirty(tmp);

                Debug.Log($"[Fixup] DamageNumber prefab: font={_cachedFont.name}");
            }

            // Fix MeshRenderer material to match font
            var mr = prefabRoot.GetComponent<MeshRenderer>();
            if (mr != null && _cachedFont != null && _cachedFont.material != null)
            {
                mr.sharedMaterial = _cachedFont.material;
                mr.sortingOrder = 20;
                EditorUtility.SetDirty(mr);
                Debug.Log("[Fixup] DamageNumber MeshRenderer: material and sorting fixed");
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        static void FixEnemyPrefab()
        {
            string prefabPath = "Assets/Prefabs/Enemies/EnemyPrefab.prefab";
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogWarning("[Fixup] Enemy prefab not found");
                return;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            // Verify SpriteRenderer
            var sr = prefabRoot.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 5;
                sr.color = Color.white;
                prefabRoot.transform.localScale = new Vector3(3, 3, 1);
                EditorUtility.SetDirty(sr);
                Debug.Log("[Fixup] Enemy prefab: sortingOrder=5, scale=3, color=white");
            }

            // Wire _spriteRenderer on Enemy component
            var enemy = prefabRoot.GetComponent<Ascendant.Combat.Enemy>();
            if (enemy != null && sr != null)
            {
                var so = new SerializedObject(enemy);
                var prop = so.FindProperty("_spriteRenderer");
                if (prop != null)
                {
                    prop.objectReferenceValue = sr;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    Debug.Log("[Fixup] Enemy: _spriteRenderer wired");
                }
            }

            // Fix HealthBarCanvas
            var canvasChild = prefabRoot.transform.Find("HealthBarCanvas");
            if (canvasChild != null)
            {
                var canvas = canvasChild.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.sortingOrder = 10;
                    EditorUtility.SetDirty(canvas);
                }

                var rt = canvasChild.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    rt.anchoredPosition = new Vector2(0f, 0.5f);
                    rt.sizeDelta = new Vector2(100f, 15f);
                    EditorUtility.SetDirty(rt);
                }

                // Fix child scales (remove 50x50 hack)
                foreach (Transform child in canvasChild)
                {
                    var childRt = child.GetComponent<RectTransform>();
                    if (childRt != null)
                    {
                        childRt.localScale = Vector3.one;
                        EditorUtility.SetDirty(childRt);
                    }
                }

                Debug.Log("[Fixup] Enemy health bar canvas fixed");
            }

            // Wire EnemyHealthBar slider and fill
            var healthBar = prefabRoot.GetComponent<Ascendant.UI.EnemyHealthBar>();
            if (healthBar != null && canvasChild != null)
            {
                var slider = canvasChild.GetComponentInChildren<Slider>();
                var so2 = new SerializedObject(healthBar);

                var sliderProp = so2.FindProperty("_slider");
                if (sliderProp != null && slider != null)
                    sliderProp.objectReferenceValue = slider;

                var fillObj = canvasChild.transform.Find("Slider/Fill Area/Fill");
                if (fillObj != null)
                {
                    var fillImage = fillObj.GetComponent<Image>();
                    var fillProp = so2.FindProperty("_fillImage");
                    if (fillProp != null && fillImage != null)
                        fillProp.objectReferenceValue = fillImage;
                }

                so2.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[Fixup] EnemyHealthBar: references wired");
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            FixEnemyDataSprites();
            AssetDatabase.SaveAssets();
        }

        static void FixEnemyDataSprites()
        {
            var spriteMap = new[] {
                ("Slime", "Assets/Art/Sprites/Enemies/Slime.png"),
                ("Goblin", "Assets/Art/Sprites/Enemies/Goblin.png"),
                ("Wolf", "Assets/Art/Sprites/Enemies/Wolf.png"),
            };

            string[] guids = AssetDatabase.FindAssets("t:Ascendant.Combat.EnemyData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<Ascendant.Combat.EnemyData>(path);
                if (data == null) continue;

                foreach (var (name, spritePath) in spriteMap)
                {
                    if (data.enemyName != null && data.enemyName.Contains(name))
                    {
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        if (sprite != null)
                        {
                            data.sprite = sprite;
                            EditorUtility.SetDirty(data);
                            Debug.Log($"[Fixup] EnemyData {data.enemyName}: sprite={name}");
                        }
                    }
                }
            }
        }

        static void FixScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

            FixCombatCanvas();
            FixAllTMPFonts();
            FixGameManagerCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Fixup] Scene saved!");
        }

        static void FixCombatCanvas()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas.gameObject.name != "CombatCanvas") continue;

                // Fix the critical scale=(0,0,0) bug — force to (1,1,1)
                var rt = canvas.GetComponent<RectTransform>();
                if (rt != null)
                {
                    var oldScale = rt.localScale;
                    rt.localScale = Vector3.one;
                    EditorUtility.SetDirty(rt);
                    Debug.Log($"[Fixup] CombatCanvas: scale {oldScale} -> (1,1,1)");
                }

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                EditorUtility.SetDirty(canvas);

                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 0.5f;
                    EditorUtility.SetDirty(scaler);
                }

                Debug.Log("[Fixup] CombatCanvas: overlay, sortOrder=100, scaler configured");
                break;
            }
        }

        static void FixAllTMPFonts()
        {
            if (_cachedFont == null) return;

            int fixedCount = 0;

            // Use Resources.FindObjectsOfTypeAll to find ALL loaded TMP components
            // (including inactive objects that FindObjectsByType misses in batch mode)
            var allUGUI = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            Debug.Log($"[Fixup] Found {allUGUI.Length} TextMeshProUGUI components");
            foreach (var tmp in allUGUI)
            {
                // Skip assets (only fix scene objects)
                if (EditorUtility.IsPersistent(tmp)) continue;

                var so = new SerializedObject(tmp);
                var fontProp = so.FindProperty("m_fontAsset");
                if (fontProp != null && fontProp.objectReferenceValue == null)
                {
                    fontProp.objectReferenceValue = _cachedFont;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(tmp);
                    fixedCount++;
                    Debug.Log($"[Fixup] Font -> {tmp.gameObject.name}");
                }
            }

            var allWorld = Resources.FindObjectsOfTypeAll<TextMeshPro>();
            Debug.Log($"[Fixup] Found {allWorld.Length} TextMeshPro (world) components");
            foreach (var tmp in allWorld)
            {
                if (EditorUtility.IsPersistent(tmp)) continue;

                var so = new SerializedObject(tmp);
                var fontProp = so.FindProperty("m_fontAsset");
                if (fontProp != null && fontProp.objectReferenceValue == null)
                {
                    fontProp.objectReferenceValue = _cachedFont;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(tmp);
                    fixedCount++;
                    Debug.Log($"[Fixup] Font -> {tmp.gameObject.name} (world)");
                }
            }

            Debug.Log($"[Fixup] TMP fonts: fixed {fixedCount} text components");
        }

        static void FixGameManagerCamera()
        {
            var gm = Object.FindAnyObjectByType<Core.GameManager>();
            if (gm == null) return;

            var cam = Object.FindAnyObjectByType<Camera>();
            if (cam == null) return;

            var so = new SerializedObject(gm);
            var camProp = so.FindProperty("_mainCamera");
            if (camProp != null && camProp.objectReferenceValue == null)
            {
                camProp.objectReferenceValue = cam;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[Fixup] GameManager: _mainCamera wired");
            }
        }
    }
}
