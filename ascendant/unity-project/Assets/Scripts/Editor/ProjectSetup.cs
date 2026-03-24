#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using Ascendant.Heroes;
using Ascendant.Combat;
using Ascendant.Progression;

namespace Ascendant.Editor
{
    public static class ProjectSetup
    {
        // Entry point: run via -executeMethod Ascendant.Editor.ProjectSetup.SetupAll
        [MenuItem("Ascendant/Full Project Setup")]
        public static void SetupAll()
        {
            Debug.Log("[ProjectSetup] Starting full project setup...");

            EnsureDirectories();
            CreatePlaceholderSprites();
            CreateURPAssets();
            CreateScriptableObjects();
            CreatePrefabs();
            CreateScenes();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ProjectSetup] Full project setup complete!");
        }

        static void EnsureDirectories()
        {
            string[] dirs = {
                "Assets/Art", "Assets/Art/Sprites", "Assets/Art/Sprites/Heroes",
                "Assets/Art/Sprites/Enemies", "Assets/Art/Sprites/UI",
                "Assets/Settings",
                "Assets/Data/Heroes", "Assets/Data/Enemies", "Assets/Data/Abilities",
                "Assets/Data/Expeditions",
                "Assets/Prefabs/Heroes", "Assets/Prefabs/Enemies", "Assets/Prefabs/UI",
                "Assets/Scenes",
                "Assets/Resources"
            };

            foreach (var dir in dirs)
            {
                string[] parts = dir.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }

        // --- Placeholder Sprites ---

        static Texture2D CreateCircleTexture(int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = size / 2f - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= radius)
                        tex.SetPixel(x, y, color);
                    else if (dist <= radius + 1.5f)
                        tex.SetPixel(x, y, Color.black); // border
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            return tex;
        }

        static Texture2D CreateSquareTexture(int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            int border = 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x < border || x >= size - border || y < border || y >= size - border)
                        tex.SetPixel(x, y, Color.black);
                    else
                        tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
            return tex;
        }

        static void SaveTextureAsSprite(Texture2D tex, string path, int pixelsPerUnit = 64)
        {
            byte[] png = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        static void CreatePlaceholderSprites()
        {
            // Hero circles (64x64)
            SaveTextureAsSprite(CreateCircleTexture(64, new Color(1f, 0.84f, 0f)), "Assets/Art/Sprites/Heroes/Warrior.png");
            SaveTextureAsSprite(CreateCircleTexture(64, new Color(0.3f, 0.5f, 1f)), "Assets/Art/Sprites/Heroes/Mage.png");
            SaveTextureAsSprite(CreateCircleTexture(64, new Color(1f, 1f, 1f)), "Assets/Art/Sprites/Heroes/Priest.png");
            SaveTextureAsSprite(CreateCircleTexture(64, new Color(0.6f, 0.2f, 0.8f)), "Assets/Art/Sprites/Heroes/Rogue.png");

            // Enemy squares (64x64)
            SaveTextureAsSprite(CreateSquareTexture(64, new Color(0.2f, 0.8f, 0.2f)), "Assets/Art/Sprites/Enemies/Slime.png");
            SaveTextureAsSprite(CreateSquareTexture(64, new Color(0.9f, 0.2f, 0.2f)), "Assets/Art/Sprites/Enemies/Goblin.png");
            SaveTextureAsSprite(CreateSquareTexture(64, new Color(0.5f, 0.5f, 0.5f)), "Assets/Art/Sprites/Enemies/Wolf.png");

            // UI sprites
            SaveTextureAsSprite(CreateSquareTexture(32, Color.white), "Assets/Art/Sprites/UI/WhiteSquare.png", 32);

            Debug.Log("[ProjectSetup] Placeholder sprites created.");
        }

        // --- URP Assets ---

        static void CreateURPAssets()
        {
            // Create URP 2D Renderer
            var renderer2D = ScriptableObject.CreateInstance<Renderer2DData>();
            AssetDatabase.CreateAsset(renderer2D, "Assets/Settings/Renderer2D.asset");

            // Create URP Pipeline Asset referencing the renderer
            var pipelineAsset = UniversalRenderPipelineAsset.Create(renderer2D);
            AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/URPPipelineAsset.asset");

            // Set as active render pipeline
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;

            Debug.Log("[ProjectSetup] URP 2D pipeline configured.");
        }

        // --- ScriptableObjects ---

        static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void CreateScriptableObjects()
        {
            // --- Hero Data ---
            CreateHeroData("Warrior", "warrior", HeroRole.Vanguard, HeroPosition.Frontline,
                Affinity.Flame, 15f, 10f, 100f, 1f,
                StatGrowth.Medium, StatGrowth.High, StatGrowth.High, StatGrowth.Low,
                5f, "Assets/Art/Sprites/Heroes/Warrior.png", "Assets/Data/Heroes/WarriorHeroData.asset");

            CreateHeroData("Mage", "mage", HeroRole.Caster, HeroPosition.Backline,
                Affinity.Frost, 18f, 6f, 80f, 0.8f,
                StatGrowth.High, StatGrowth.Low, StatGrowth.Low, StatGrowth.Medium,
                8f, "Assets/Art/Sprites/Heroes/Mage.png", "Assets/Data/Heroes/MageHeroData.asset");

            CreateHeroData("Priest", "priest", HeroRole.Support, HeroPosition.Backline,
                Affinity.Radiance, 10f, 8f, 110f, 0.7f,
                StatGrowth.Low, StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Low,
                4f, "Assets/Art/Sprites/Heroes/Priest.png", "Assets/Data/Heroes/PriestHeroData.asset");

            CreateHeroData("Rogue", "rogue", HeroRole.Striker, HeroPosition.Frontline,
                Affinity.Shadow, 20f, 7f, 85f, 1.3f,
                StatGrowth.High, StatGrowth.Low, StatGrowth.Low, StatGrowth.High,
                7f, "Assets/Art/Sprites/Heroes/Rogue.png", "Assets/Data/Heroes/RogueHeroData.asset");

            // --- Enemy Data ---
            CreateEnemyData("Slime", Affinity.Nature, EnemyCategory.Beast, EnemyAttackType.Melee,
                50f, 5f, 2f, 10f, 5f,
                "Assets/Art/Sprites/Enemies/Slime.png", "Assets/Data/Enemies/SlimeEnemyData.asset");

            CreateEnemyData("Goblin", Affinity.None, EnemyCategory.Humanoid, EnemyAttackType.Melee,
                40f, 8f, 3f, 12f, 6f,
                "Assets/Art/Sprites/Enemies/Goblin.png", "Assets/Data/Enemies/GoblinEnemyData.asset");

            CreateEnemyData("Wolf", Affinity.None, EnemyCategory.Beast, EnemyAttackType.Melee,
                35f, 10f, 1f, 8f, 4f,
                "Assets/Art/Sprites/Enemies/Wolf.png", "Assets/Data/Enemies/WolfEnemyData.asset");

            // --- Warrior Abilities ---
            CreateAbility("Cleaving Strike", "200% ATK to all enemies in a line", 0,
                2f, 8f, AbilityTargetType.AllEnemies, false, 0f, 0f, 0f, 100f,
                "Assets/Data/Abilities/Warrior_CleavingStrike.asset");

            CreateAbility("War Cry", "+15% ATK to all allies for 8s", 1,
                0f, 15f, AbilityTargetType.PartyBuff, false, 0.15f, 8f, 0f, 100f,
                "Assets/Data/Abilities/Warrior_WarCry.asset");

            CreateAbility("Ascendant Blade", "500% ATK to single enemy + stun 2s", 2,
                5f, 0f, AbilityTargetType.SingleEnemy, true, 0f, 0f, 2f, 100f,
                "Assets/Data/Abilities/Warrior_AscendantBlade.asset");

            // --- Progression Config ---
            var config = ScriptableObject.CreateInstance<ProgressionConfig>();
            config.enemyHpScalePerStage = 0.08f;
            config.enemyAtkScalePerStage = 0.05f;
            config.goldScalePerStage = 0.06f;
            config.xpScalePerStage = 0.04f;
            config.minEnemiesPerWave = 3;
            config.maxEnemiesPerWave = 5;
            config.stagesPerIsland = 100;
            config.miniBossEveryNStages = 10;
            config.stageTransitionDelay = 1f;
            AssetDatabase.CreateAsset(config, "Assets/Data/ProgressionConfig.asset");

            Debug.Log("[ProjectSetup] ScriptableObjects created.");
        }

        static void CreateHeroData(string heroName, string classId, HeroRole role, HeroPosition position,
            Affinity affinity, float baseAtk, float baseDef, float baseHp, float baseSpd,
            StatGrowth atkGrowth, StatGrowth defGrowth, StatGrowth hpGrowth, StatGrowth spdGrowth,
            float baseTapBonus, string spritePath, string assetPath)
        {
            var data = ScriptableObject.CreateInstance<HeroData>();
            data.heroName = heroName;
            data.className = heroName;
            data.classId = classId;
            data.role = role;
            data.position = position;
            data.affinity = affinity;
            data.baseAtk = baseAtk;
            data.baseDef = baseDef;
            data.baseHp = baseHp;
            data.baseSpd = baseSpd;
            data.atkGrowth = atkGrowth;
            data.defGrowth = defGrowth;
            data.hpGrowth = hpGrowth;
            data.spdGrowth = spdGrowth;
            data.baseTapBonus = baseTapBonus;
            data.portrait = LoadSprite(spritePath);
            AssetDatabase.CreateAsset(data, assetPath);
        }

        static void CreateEnemyData(string enemyName, Affinity affinity, EnemyCategory category,
            EnemyAttackType attackType, float baseHp, float baseAtk, float baseDef,
            float baseGold, float baseXp, string spritePath, string assetPath)
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.enemyName = enemyName;
            data.affinity = affinity;
            data.category = category;
            data.attackType = attackType;
            data.baseHp = baseHp;
            data.baseAtk = baseAtk;
            data.baseDef = baseDef;
            data.baseGoldDrop = baseGold;
            data.baseXpDrop = baseXp;
            data.sprite = LoadSprite(spritePath);
            AssetDatabase.CreateAsset(data, assetPath);
        }

        static void CreateAbility(string name, string description, int slotIndex,
            float damageMultiplier, float cooldown, AbilityTargetType targetType,
            bool isUltimate, float buffMultiplier, float buffDuration, float stunDuration,
            float chargeRequired, string assetPath)
        {
            var ability = ScriptableObject.CreateInstance<Ability>();
            ability.abilityName = name;
            ability.description = description;
            ability.slotIndex = slotIndex;
            ability.damageMultiplier = damageMultiplier;
            ability.cooldown = cooldown;
            ability.targetType = targetType;
            ability.isUltimate = isUltimate;
            ability.buffMultiplier = buffMultiplier;
            ability.buffDuration = buffDuration;
            ability.stunDuration = stunDuration;
            ability.chargeRequired = chargeRequired;
            AssetDatabase.CreateAsset(ability, assetPath);
        }

        // --- Prefabs ---

        static void CreatePrefabs()
        {
            CreateEnemyPrefab();
            CreateDamageNumberPrefab();
            Debug.Log("[ProjectSetup] Prefabs created.");
        }

        static void CreateEnemyPrefab()
        {
            var enemyGO = new GameObject("Enemy");

            // Sprite
            var sr = enemyGO.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Art/Sprites/Enemies/Slime.png");
            sr.sortingOrder = 1;

            // Enemy component
            var enemy = enemyGO.AddComponent<Enemy>();
            // Set _spriteRenderer via SerializedObject
            var so = new SerializedObject(enemy);
            so.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            so.ApplyModifiedPropertiesWithoutUndo();

            // World-space health bar canvas
            var hbCanvasGO = new GameObject("HealthBarCanvas");
            hbCanvasGO.transform.SetParent(enemyGO.transform);
            var hbCanvas = hbCanvasGO.AddComponent<Canvas>();
            hbCanvas.renderMode = RenderMode.WorldSpace;
            hbCanvas.sortingOrder = 10;
            var hbCanvasRT = hbCanvasGO.GetComponent<RectTransform>();
            hbCanvasRT.sizeDelta = new Vector2(1f, 0.2f);
            hbCanvasRT.localPosition = new Vector3(0f, 0.7f, 0f);
            hbCanvasRT.localScale = Vector3.one * 0.02f;

            var hbBg = new GameObject("Background");
            hbBg.transform.SetParent(hbCanvasGO.transform);
            var hbBgImg = hbBg.AddComponent<Image>();
            hbBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var hbBgRT = hbBg.GetComponent<RectTransform>();
            hbBgRT.anchorMin = Vector2.zero;
            hbBgRT.anchorMax = Vector2.one;
            hbBgRT.offsetMin = Vector2.zero;
            hbBgRT.offsetMax = Vector2.zero;

            var hbSliderGO = new GameObject("Slider");
            hbSliderGO.transform.SetParent(hbCanvasGO.transform);
            var slider = hbSliderGO.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            var sliderRT = hbSliderGO.GetComponent<RectTransform>();
            sliderRT.anchorMin = Vector2.zero;
            sliderRT.anchorMax = Vector2.one;
            sliderRT.offsetMin = Vector2.zero;
            sliderRT.offsetMax = Vector2.zero;

            // Fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(hbSliderGO.transform);
            var fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.offsetMin = Vector2.zero;
            fillAreaRT.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.green;
            var fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            slider.fillRect = fillRT;

            // EnemyHealthBar component
            var ehb = enemyGO.AddComponent<Ascendant.UI.EnemyHealthBar>();
            var ehbSO = new SerializedObject(ehb);
            ehbSO.FindProperty("_slider").objectReferenceValue = slider;
            ehbSO.FindProperty("_fillImage").objectReferenceValue = fillImg;
            ehbSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(enemyGO, "Assets/Prefabs/Enemies/EnemyPrefab.prefab");
            Object.DestroyImmediate(enemyGO);
        }

        static void CreateDamageNumberPrefab()
        {
            var dmgGO = new GameObject("DamageNumber");

            var tmp = dmgGO.AddComponent<TextMeshPro>();
            tmp.fontSize = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.sortingOrder = 20;

            var dmgNum = dmgGO.AddComponent<Ascendant.UI.DamageNumber>();
            var so = new SerializedObject(dmgNum);
            so.FindProperty("_text").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(dmgGO, "Assets/Prefabs/UI/DamageNumberPrefab.prefab");
            Object.DestroyImmediate(dmgGO);
        }

        // --- Scenes ---

        static void CreateScenes()
        {
            CreateBootScene();
            CreateMainMenuScene();
            CreateGameScene();
            Debug.Log("[ProjectSetup] Scenes created.");
        }

        static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.tag = "MainCamera";
            camGO.AddComponent<UniversalAdditionalCameraData>();

            // Boot script
            var bootGO = new GameObject("BootLoader");
            bootGO.AddComponent<Ascendant.Core.BootLoader>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/BootScene.unity");
        }

        static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.15f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.tag = "MainCamera";
            camGO.AddComponent<UniversalAdditionalCameraData>();

            // Canvas
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvasGO.transform, false);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "ASCENDANT";
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.85f, 0f);
            titleText.fontStyle = FontStyles.Bold;
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.55f);
            titleRT.anchorMax = new Vector2(0.9f, 0.75f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            // Subtitle
            var subtitleGO = new GameObject("Subtitle");
            subtitleGO.transform.SetParent(canvasGO.transform, false);
            var subtitleText = subtitleGO.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "Hero Simulator";
            subtitleText.fontSize = 28;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.color = new Color(0.7f, 0.7f, 0.8f);
            var subtitleRT = subtitleGO.GetComponent<RectTransform>();
            subtitleRT.anchorMin = new Vector2(0.2f, 0.48f);
            subtitleRT.anchorMax = new Vector2(0.8f, 0.55f);
            subtitleRT.offsetMin = Vector2.zero;
            subtitleRT.offsetMax = Vector2.zero;

            // Play button
            var btnGO = new GameObject("PlayButton");
            btnGO.transform.SetParent(canvasGO.transform, false);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 1f);
            var btn = btnGO.AddComponent<Button>();
            var btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.25f, 0.28f);
            btnRT.anchorMax = new Vector2(0.75f, 0.38f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            var btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
            btnText.text = "PLAY";
            btnText.fontSize = 36;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyles.Bold;
            var btnTextRT = btnTextGO.GetComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = Vector2.zero;
            btnTextRT.offsetMax = Vector2.zero;

            // Add MainMenu script
            var menuGO = new GameObject("MainMenu");
            menuGO.AddComponent<Ascendant.UI.MainMenuController>();

            // Wire button to MainMenu
            var menuController = menuGO.GetComponent<Ascendant.UI.MainMenuController>();
            var menuSO = new SerializedObject(menuController);
            menuSO.FindProperty("_playButton").objectReferenceValue = btn;
            menuSO.ApplyModifiedPropertiesWithoutUndo();

            // EventSystem
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenuScene.unity");
        }

        static void CreateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Camera ---
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8; // portrait: wider view
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.18f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.tag = "MainCamera";
            camGO.AddComponent<UniversalAdditionalCameraData>();

            // --- Combat Background ---
            var bgGO = new GameObject("CombatBackground");
            var bgSR = bgGO.AddComponent<SpriteRenderer>();
            bgSR.color = new Color(0.12f, 0.15f, 0.25f);
            bgSR.sortingOrder = -10;
            // Create a simple sprite from the white square
            bgSR.sprite = LoadSprite("Assets/Art/Sprites/UI/WhiteSquare.png");
            bgGO.transform.localScale = new Vector3(20f, 30f, 1f);

            // --- Managers Root ---
            var managersGO = new GameObject("--- MANAGERS ---");

            // GameManager
            var gmGO = new GameObject("GameManager");
            gmGO.transform.SetParent(managersGO.transform);
            var gm = gmGO.AddComponent<Ascendant.Core.GameManager>();
            var gmSO = new SerializedObject(gm);
            gmSO.FindProperty("_mainCamera").objectReferenceValue = cam;
            gmSO.ApplyModifiedPropertiesWithoutUndo();

            // SaveManager
            var saveGO = new GameObject("SaveManager");
            saveGO.transform.SetParent(managersGO.transform);
            saveGO.AddComponent<Ascendant.Core.SaveManager>();

            // CurrencyManager
            var currGO = new GameObject("CurrencyManager");
            currGO.transform.SetParent(managersGO.transform);
            currGO.AddComponent<Ascendant.Economy.CurrencyManager>();

            // EnemyManager
            var emGO = new GameObject("EnemyManager");
            emGO.transform.SetParent(managersGO.transform);
            emGO.AddComponent<Ascendant.Combat.EnemyManager>();

            // MomentumSystem
            var momGO = new GameObject("MomentumSystem");
            momGO.transform.SetParent(managersGO.transform);
            var momentum = momGO.AddComponent<Ascendant.Combat.MomentumSystem>();

            // HeroManager
            var hmGO = new GameObject("HeroManager");
            hmGO.transform.SetParent(managersGO.transform);
            hmGO.AddComponent<Ascendant.Heroes.HeroManager>();

            // PartyManager with hero instances
            var pmGO = new GameObject("PartyManager");
            pmGO.transform.SetParent(managersGO.transform);
            var partyManager = pmGO.AddComponent<Ascendant.Party.PartyManager>();

            // Create 4 hero GameObjects as children of PartyManager
            var heroNames = new[] { "Warrior", "Mage", "Priest", "Rogue" };
            var heroPositions = new[] {
                new Vector3(-2f, -2f, 0f),  // Front Left
                new Vector3(-1f, -2f, 0f),  // Front Right
                new Vector3(-2.5f, -3.5f, 0f), // Back Left
                new Vector3(-0.5f, -3.5f, 0f)  // Back Right
            };
            var heroRefs = new Hero[4];

            for (int i = 0; i < 4; i++)
            {
                var heroGO = new GameObject($"Hero_{heroNames[i]}");
                heroGO.transform.SetParent(pmGO.transform);
                heroGO.transform.position = heroPositions[i];

                var heroSR = heroGO.AddComponent<SpriteRenderer>();
                heroSR.sprite = LoadSprite($"Assets/Art/Sprites/Heroes/{heroNames[i]}.png");
                heroSR.sortingOrder = 5;

                var hero = heroGO.AddComponent<Hero>();
                var heroData = AssetDatabase.LoadAssetAtPath<HeroData>($"Assets/Data/Heroes/{heroNames[i]}HeroData.asset");

                var heroSO = new SerializedObject(hero);
                heroSO.FindProperty("_data").objectReferenceValue = heroData;
                heroSO.FindProperty("_slot").intValue = i;
                heroSO.ApplyModifiedPropertiesWithoutUndo();

                // Add tap mechanic
                switch (heroNames[i])
                {
                    case "Warrior":
                        heroGO.AddComponent<WarriorTapMechanic>();
                        break;
                    case "Mage":
                        heroGO.AddComponent<MageTapMechanic>();
                        break;
                    case "Priest":
                        heroGO.AddComponent<PriestTapMechanic>();
                        break;
                    case "Rogue":
                        heroGO.AddComponent<RogueTapMechanic>();
                        break;
                }

                heroRefs[i] = hero;
            }

            // Wire PartyManager._partySlots
            var pmSO = new SerializedObject(partyManager);
            var slotsArray = pmSO.FindProperty("_partySlots");
            slotsArray.arraySize = 4;
            for (int i = 0; i < 4; i++)
                slotsArray.GetArrayElementAtIndex(i).objectReferenceValue = heroRefs[i];
            pmSO.ApplyModifiedPropertiesWithoutUndo();

            // AutoAttackSystem
            var autoAtkGO = new GameObject("AutoAttackSystem");
            autoAtkGO.transform.SetParent(managersGO.transform);
            autoAtkGO.AddComponent<Ascendant.Combat.AutoAttackSystem>();

            // TapInputController
            var tapGO = new GameObject("TapInputController");
            tapGO.transform.SetParent(managersGO.transform);
            var tap = tapGO.AddComponent<Ascendant.Combat.TapInputController>();
            var tapSO = new SerializedObject(tap);
            tapSO.FindProperty("_momentum").objectReferenceValue = momentum;
            tapSO.ApplyModifiedPropertiesWithoutUndo();

            // EnemySpawner
            var spawnerGO = new GameObject("EnemySpawner");
            spawnerGO.transform.SetParent(managersGO.transform);
            var spawner = spawnerGO.AddComponent<Ascendant.Combat.EnemySpawner>();

            // Load enemy data and prefab
            var slimeData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/Data/Enemies/SlimeEnemyData.asset");
            var goblinData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/Data/Enemies/GoblinEnemyData.asset");
            var wolfData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/Data/Enemies/WolfEnemyData.asset");
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<Enemy>("Assets/Prefabs/Enemies/EnemyPrefab.prefab");

            var spawnerSO = new SerializedObject(spawner);
            spawnerSO.FindProperty("_enemyPrefab").objectReferenceValue = enemyPrefab;
            var enemyTypesList = spawnerSO.FindProperty("_enemyTypes");
            enemyTypesList.arraySize = 3;
            enemyTypesList.GetArrayElementAtIndex(0).objectReferenceValue = slimeData;
            enemyTypesList.GetArrayElementAtIndex(1).objectReferenceValue = goblinData;
            enemyTypesList.GetArrayElementAtIndex(2).objectReferenceValue = wolfData;
            spawnerSO.ApplyModifiedPropertiesWithoutUndo();

            // StageManager
            var stageGO = new GameObject("StageManager");
            stageGO.transform.SetParent(managersGO.transform);
            var stageMgr = stageGO.AddComponent<Ascendant.Progression.StageManager>();
            var progressionConfig = AssetDatabase.LoadAssetAtPath<ProgressionConfig>("Assets/Data/ProgressionConfig.asset");
            var stageSO = new SerializedObject(stageMgr);
            stageSO.FindProperty("_config").objectReferenceValue = progressionConfig;
            stageSO.FindProperty("_spawner").objectReferenceValue = spawner;
            stageSO.ApplyModifiedPropertiesWithoutUndo();

            // Idle systems
            var afkGO = new GameObject("AFKVaultSystem");
            afkGO.transform.SetParent(managersGO.transform);
            afkGO.AddComponent<Ascendant.Idle.AFKVaultSystem>();

            var expedGO = new GameObject("ExpeditionManager");
            expedGO.transform.SetParent(managersGO.transform);
            expedGO.AddComponent<Ascendant.Idle.ExpeditionManager>();

            var notifSettingsGO = new GameObject("NotificationSettings");
            notifSettingsGO.transform.SetParent(managersGO.transform);
            notifSettingsGO.AddComponent<Ascendant.Idle.NotificationSettings>();

            var notifGO = new GameObject("NotificationManager");
            notifGO.transform.SetParent(managersGO.transform);
            notifGO.AddComponent<Ascendant.Idle.NotificationManager>();

            // GameBootstrap (initializes heroes and tap mechanics)
            var bootstrapGO = new GameObject("GameBootstrap");
            bootstrapGO.transform.SetParent(managersGO.transform);
            bootstrapGO.AddComponent<Ascendant.Core.GameBootstrap>();

            // --- Combat UI Canvas ---
            var canvasGO = new GameObject("CombatCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // CombatUI component
            var combatUI = canvasGO.AddComponent<Ascendant.UI.CombatUI>();

            // Stage text (top-left)
            var stageTextGO = CreateUIText(canvasGO.transform, "StageText",
                "Island 1 - Stage 1/100", 24,
                new Vector2(0f, 0.93f), new Vector2(0.5f, 1f),
                TextAlignmentOptions.TopLeft);
            var stageRT = stageTextGO.GetComponent<RectTransform>();
            stageRT.offsetMin = new Vector2(20f, 0f);
            stageRT.offsetMax = new Vector2(0f, -10f);

            // Gold text (top-center-left)
            var goldTextGO = CreateUIText(canvasGO.transform, "GoldText",
                "0 Gold", 22,
                new Vector2(0.15f, 0.9f), new Vector2(0.5f, 0.93f),
                TextAlignmentOptions.Left);
            var goldTMP = goldTextGO.GetComponent<TextMeshProUGUI>();
            goldTMP.color = new Color(1f, 0.84f, 0f);

            // XP text (top-center-right)
            var xpTextGO = CreateUIText(canvasGO.transform, "XPText",
                "0 XP", 22,
                new Vector2(0.5f, 0.9f), new Vector2(0.85f, 0.93f),
                TextAlignmentOptions.Right);
            var xpTMP = xpTextGO.GetComponent<TextMeshProUGUI>();
            xpTMP.color = new Color(0.3f, 0.8f, 1f);

            // Momentum bar (left edge, vertical)
            var momBarGO = new GameObject("MomentumBar");
            momBarGO.transform.SetParent(canvasGO.transform, false);
            var momBarRT = momBarGO.AddComponent<RectTransform>();
            momBarRT.anchorMin = new Vector2(0f, 0.4f);
            momBarRT.anchorMax = new Vector2(0.04f, 0.88f);
            momBarRT.offsetMin = new Vector2(10f, 0f);
            momBarRT.offsetMax = new Vector2(0f, 0f);

            // Momentum background
            var momBgGO = new GameObject("Background");
            momBgGO.transform.SetParent(momBarGO.transform, false);
            var momBgImg = momBgGO.AddComponent<Image>();
            momBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            var momBgRT = momBgGO.GetComponent<RectTransform>();
            momBgRT.anchorMin = Vector2.zero;
            momBgRT.anchorMax = Vector2.one;
            momBgRT.offsetMin = Vector2.zero;
            momBgRT.offsetMax = Vector2.zero;

            // Momentum slider
            var momSliderGO = new GameObject("Slider");
            momSliderGO.transform.SetParent(momBarGO.transform, false);
            var momSlider = momSliderGO.AddComponent<Slider>();
            momSlider.interactable = false;
            momSlider.direction = Slider.Direction.BottomToTop;
            momSlider.transition = Selectable.Transition.None;
            var momSliderRT = momSliderGO.GetComponent<RectTransform>();
            momSliderRT.anchorMin = Vector2.zero;
            momSliderRT.anchorMax = Vector2.one;
            momSliderRT.offsetMin = Vector2.zero;
            momSliderRT.offsetMax = Vector2.zero;

            var momFillArea = new GameObject("Fill Area");
            momFillArea.transform.SetParent(momSliderGO.transform, false);
            var momFillAreaRT = momFillArea.AddComponent<RectTransform>();
            momFillAreaRT.anchorMin = Vector2.zero;
            momFillAreaRT.anchorMax = Vector2.one;
            momFillAreaRT.offsetMin = Vector2.zero;
            momFillAreaRT.offsetMax = Vector2.zero;

            var momFill = new GameObject("Fill");
            momFill.transform.SetParent(momFillArea.transform, false);
            var momFillImg = momFill.AddComponent<Image>();
            momFillImg.color = new Color(1f, 0.5f, 0f);
            var momFillRT = momFill.GetComponent<RectTransform>();
            momFillRT.anchorMin = Vector2.zero;
            momFillRT.anchorMax = Vector2.one;
            momFillRT.offsetMin = Vector2.zero;
            momFillRT.offsetMax = Vector2.zero;
            momSlider.fillRect = momFillRT;

            // Momentum text
            var momTextGO = CreateUIText(canvasGO.transform, "MomentumText",
                "x1.00", 16,
                new Vector2(0f, 0.88f), new Vector2(0.08f, 0.92f),
                TextAlignmentOptions.Center);

            // Hero portraits (bottom panel, 4 slots)
            var portraitsParent = new GameObject("HeroPortraits");
            portraitsParent.transform.SetParent(canvasGO.transform, false);
            var ppRT = portraitsParent.AddComponent<RectTransform>();
            ppRT.anchorMin = new Vector2(0.02f, 0.01f);
            ppRT.anchorMax = new Vector2(0.98f, 0.15f);
            ppRT.offsetMin = Vector2.zero;
            ppRT.offsetMax = Vector2.zero;

            // Add horizontal layout
            var hlg = portraitsParent.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var heroPortraitRefs = new Ascendant.UI.HeroPortrait[4];
            var heroColors = new[] {
                new Color(1f, 0.84f, 0f), // Warrior gold
                new Color(0.3f, 0.5f, 1f), // Mage blue
                new Color(1f, 1f, 1f),     // Priest white
                new Color(0.6f, 0.2f, 0.8f) // Rogue purple
            };

            for (int i = 0; i < 4; i++)
            {
                var portraitGO = new GameObject($"HeroPortrait_{heroNames[i]}");
                portraitGO.transform.SetParent(portraitsParent.transform, false);

                // Portrait background
                var portraitBorder = portraitGO.AddComponent<Image>();
                portraitBorder.color = heroColors[i];

                // Portrait image (child)
                var portraitImgGO = new GameObject("PortraitImage");
                portraitImgGO.transform.SetParent(portraitGO.transform, false);
                var portraitImg = portraitImgGO.AddComponent<Image>();
                portraitImg.color = heroColors[i] * 0.7f;
                var pImgRT = portraitImgGO.GetComponent<RectTransform>();
                pImgRT.anchorMin = new Vector2(0.05f, 0.15f);
                pImgRT.anchorMax = new Vector2(0.95f, 0.95f);
                pImgRT.offsetMin = Vector2.zero;
                pImgRT.offsetMax = Vector2.zero;

                // HP bar (child)
                var hpBarGO = new GameObject("HPBar");
                hpBarGO.transform.SetParent(portraitGO.transform, false);
                var hpBarRT = hpBarGO.AddComponent<RectTransform>();
                hpBarRT.anchorMin = new Vector2(0.05f, 0.02f);
                hpBarRT.anchorMax = new Vector2(0.95f, 0.12f);
                hpBarRT.offsetMin = Vector2.zero;
                hpBarRT.offsetMax = Vector2.zero;

                var hpBg = hpBarGO.AddComponent<Image>();
                hpBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

                var hpSliderGO = new GameObject("Slider");
                hpSliderGO.transform.SetParent(hpBarGO.transform, false);
                var hpSlider = hpSliderGO.AddComponent<Slider>();
                hpSlider.interactable = false;
                hpSlider.transition = Selectable.Transition.None;
                hpSlider.value = 1f;
                var hpSliderRT = hpSliderGO.GetComponent<RectTransform>();
                hpSliderRT.anchorMin = Vector2.zero;
                hpSliderRT.anchorMax = Vector2.one;
                hpSliderRT.offsetMin = Vector2.zero;
                hpSliderRT.offsetMax = Vector2.zero;

                var hpFillArea = new GameObject("Fill Area");
                hpFillArea.transform.SetParent(hpSliderGO.transform, false);
                var hpFillAreaRT = hpFillArea.AddComponent<RectTransform>();
                hpFillAreaRT.anchorMin = Vector2.zero;
                hpFillAreaRT.anchorMax = Vector2.one;
                hpFillAreaRT.offsetMin = Vector2.zero;
                hpFillAreaRT.offsetMax = Vector2.zero;

                var hpFill = new GameObject("Fill");
                hpFill.transform.SetParent(hpFillArea.transform, false);
                var hpFillImg = hpFill.AddComponent<Image>();
                hpFillImg.color = Color.green;
                var hpFillRT = hpFill.GetComponent<RectTransform>();
                hpFillRT.anchorMin = Vector2.zero;
                hpFillRT.anchorMax = Vector2.one;
                hpFillRT.offsetMin = Vector2.zero;
                hpFillRT.offsetMax = Vector2.zero;
                hpSlider.fillRect = hpFillRT;

                // HeroPortrait component
                var hp = portraitGO.AddComponent<Ascendant.UI.HeroPortrait>();
                var hpSO = new SerializedObject(hp);
                hpSO.FindProperty("_portraitImage").objectReferenceValue = portraitImg;
                hpSO.FindProperty("_portraitBorder").objectReferenceValue = portraitBorder;
                hpSO.FindProperty("_hpBar").objectReferenceValue = hpSlider;
                hpSO.FindProperty("_hpFill").objectReferenceValue = hpFillImg;
                hpSO.FindProperty("_heroSlot").intValue = i;
                hpSO.ApplyModifiedPropertiesWithoutUndo();

                heroPortraitRefs[i] = hp;
            }

            // Wire CombatUI references
            var cuiSO = new SerializedObject(combatUI);
            cuiSO.FindProperty("_stageText").objectReferenceValue = stageTextGO.GetComponent<TextMeshProUGUI>();
            cuiSO.FindProperty("_goldText").objectReferenceValue = goldTMP;
            cuiSO.FindProperty("_xpText").objectReferenceValue = xpTMP;
            cuiSO.FindProperty("_momentumBar").objectReferenceValue = momSlider;
            cuiSO.FindProperty("_momentumText").objectReferenceValue = momTextGO.GetComponent<TextMeshProUGUI>();
            var portraitsArray = cuiSO.FindProperty("_heroPortraits");
            portraitsArray.arraySize = 4;
            for (int i = 0; i < 4; i++)
                portraitsArray.GetArrayElementAtIndex(i).objectReferenceValue = heroPortraitRefs[i];
            cuiSO.ApplyModifiedPropertiesWithoutUndo();

            // Damage number pool
            var dmgPoolGO = new GameObject("DamageNumberPool");
            dmgPoolGO.transform.SetParent(canvasGO.transform, false);
            var dmgPool = dmgPoolGO.AddComponent<Ascendant.UI.DamageNumberPool>();
            var dmgPrefab = AssetDatabase.LoadAssetAtPath<Ascendant.UI.DamageNumber>("Assets/Prefabs/UI/DamageNumberPrefab.prefab");
            var dmgPoolSO = new SerializedObject(dmgPool);
            dmgPoolSO.FindProperty("_prefab").objectReferenceValue = dmgPrefab;
            dmgPoolSO.ApplyModifiedPropertiesWithoutUndo();

            // AFK Vault UI
            var vaultUIGO = new GameObject("AFKVaultUI");
            vaultUIGO.transform.SetParent(canvasGO.transform, false);
            vaultUIGO.AddComponent<Ascendant.Idle.AFKVaultUI>();
            var vaultRT = vaultUIGO.AddComponent<RectTransform>();
            vaultRT.anchorMin = new Vector2(0.85f, 0.93f);
            vaultRT.anchorMax = new Vector2(1f, 1f);
            vaultRT.offsetMin = Vector2.zero;
            vaultRT.offsetMax = Vector2.zero;

            // EventSystem
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        }

        static GameObject CreateUIText(Transform parent, string name, string text, float fontSize,
            Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        static void ConfigureBuildSettings()
        {
            var scenes = new[] {
                new EditorBuildSettingsScene("Assets/Scenes/BootScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[ProjectSetup] Build settings configured with 3 scenes.");
        }
    }
}
#endif
