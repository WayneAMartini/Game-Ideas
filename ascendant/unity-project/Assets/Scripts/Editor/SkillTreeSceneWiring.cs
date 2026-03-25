#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Ascendant.EditorTools
{
    public static class SkillTreeSceneWiring
    {
        static TMP_FontAsset _font;
        static readonly Color DarkOverlay = new Color(0.05f, 0.05f, 0.1f, 0.97f);
        static readonly Color PanelBg = new Color(0.12f, 0.12f, 0.18f, 1f);
        static readonly Color ButtonBg = new Color(0.18f, 0.18f, 0.25f, 1f);
        static readonly Color TabActive = new Color(0.2f, 0.4f, 0.9f, 1f);
        static readonly Color TabInactive = new Color(0.25f, 0.25f, 0.3f, 1f);
        static readonly Color AccentGold = new Color(1f, 0.85f, 0.3f, 1f);

        [MenuItem("Ascendant/Wire Skill Tree Scene")]
        public static void WireAll()
        {
            Debug.Log("=== SkillTreeSceneWiring: Starting ===");

            _font = FindFont();
            if (_font == null)
            {
                Debug.LogError("[SkillTreeSceneWiring] No TMP font found. Aborting.");
                return;
            }

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

            var canvas = FindCombatCanvas();
            if (canvas == null)
            {
                Debug.LogError("[SkillTreeSceneWiring] CombatCanvas not found. Aborting.");
                return;
            }

            // A) Wire SkillTreeSystem on MANAGERS
            WireSkillTreeSystem();

            // B) Create SkillNodeButton and ConnectionLine prefabs
            var nodePrefab = CreateSkillNodeButtonPrefab();
            var linePrefab = CreateConnectionLinePrefab();

            // C) Create SkillTreePanel
            var skillTreePanel = CreateSkillTreePanel(canvas.transform, nodePrefab, linePrefab);

            // D) Wire HeroesPanelController
            WireHeroesPanelController(canvas.transform, skillTreePanel);

            // E) Remove "Coming Soon" from HeroDetail
            RemoveComingSoonLabel(canvas.transform);

            // F) Save
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("=== SkillTreeSceneWiring: Done ===");
        }

        // ── A) Wire SkillTreeSystem ──────────────────────────────────
        static void WireSkillTreeSystem()
        {
            // Find MANAGERS object
            GameObject managers = GameObject.Find("MANAGERS");
            if (managers == null)
            {
                // Try to find it in root objects
                foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (root.name == "MANAGERS")
                    {
                        managers = root;
                        break;
                    }
                }
            }
            if (managers == null)
            {
                managers = new GameObject("MANAGERS");
                Debug.Log("[SkillTreeSceneWiring] Created MANAGERS object");
            }

            var system = managers.GetComponent<Progression.SkillTreeSystem>();
            if (system == null)
                system = managers.AddComponent<Progression.SkillTreeSystem>();

            // Load all 24 SkillTreeData assets
            string[] guids = AssetDatabase.FindAssets("t:Ascendant.Progression.SkillTreeData", new[] { "Assets/Data/SkillTrees" });
            var trees = new List<Progression.SkillTreeData>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tree = AssetDatabase.LoadAssetAtPath<Progression.SkillTreeData>(path);
                if (tree != null)
                    trees.Add(tree);
            }

            // Wire via SerializedObject
            var so = new SerializedObject(system);
            var treesProp = so.FindProperty("_skillTrees");
            if (treesProp != null)
            {
                treesProp.arraySize = trees.Count;
                for (int i = 0; i < trees.Count; i++)
                    treesProp.GetArrayElementAtIndex(i).objectReferenceValue = trees[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(system);
            Debug.Log($"[SkillTreeSceneWiring] SkillTreeSystem wired with {trees.Count} trees");
        }

        // ── B) Create Prefabs ────────────────────────────────────────
        static GameObject CreateSkillNodeButtonPrefab()
        {
            string prefabPath = "Assets/Prefabs/UI/SkillNodeButton.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return existing;

            var obj = new GameObject("SkillNodeButton", typeof(RectTransform));
            var rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.4f, 1f);

            var btn = obj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;

            var textObj = new GameObject("Label", typeof(RectTransform));
            textObj.transform.SetParent(obj.transform, false);
            var trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(2, 2);
            trt.offsetMax = new Vector2(-2, -2);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Node";
            tmp.fontSize = 12;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            if (_font != null)
            {
                var tmpSo = new SerializedObject(tmp);
                var fontProp = tmpSo.FindProperty("m_fontAsset");
                if (fontProp != null) fontProp.objectReferenceValue = _font;
                tmpSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // Save as prefab
            EnsureDirectory("Assets/Prefabs/UI");
            var prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            Object.DestroyImmediate(obj);
            Debug.Log("[SkillTreeSceneWiring] SkillNodeButton prefab created");
            return prefab;
        }

        static GameObject CreateConnectionLinePrefab()
        {
            string prefabPath = "Assets/Prefabs/UI/ConnectionLine.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return existing;

            var obj = new GameObject("ConnectionLine", typeof(RectTransform));
            var rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 4);
            rt.pivot = new Vector2(0.5f, 0.5f);

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.5f, 0.5f, 0.6f, 0.8f);

            EnsureDirectory("Assets/Prefabs/UI");
            var prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            Object.DestroyImmediate(obj);
            Debug.Log("[SkillTreeSceneWiring] ConnectionLine prefab created");
            return prefab;
        }

        // ── C) Create SkillTreePanel ─────────────────────────────────
        static GameObject CreateSkillTreePanel(Transform canvasTransform, GameObject nodePrefab, GameObject linePrefab)
        {
            // Clean up existing
            DestroyChild(canvasTransform, "SkillTreePanel");

            var panel = CreateUIObject(canvasTransform, "SkillTreePanel", Vector2.zero);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            // Dark overlay background
            AddImage(panel, DarkOverlay);
            panel.SetActive(false); // hidden by default

            // Header bar with title + close button
            var header = CreateUIObject(panel.transform, "Header", Vector2.zero);
            var hrt = header.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, 1);
            hrt.anchorMax = new Vector2(1, 1);
            hrt.pivot = new Vector2(0.5f, 1);
            hrt.anchoredPosition = Vector2.zero;
            hrt.sizeDelta = new Vector2(0, 80);
            AddImage(header, PanelBg);

            var titleText = CreateText(header.transform, "TitleText", "SKILL TREE", 30, TextAlignmentOptions.Center);
            var ttrt = titleText.GetComponent<RectTransform>();
            ttrt.anchorMin = Vector2.zero;
            ttrt.anchorMax = Vector2.one;
            ttrt.offsetMin = Vector2.zero;
            ttrt.offsetMax = Vector2.zero;

            // Close button
            var closeBtn = CreateUIObject(header.transform, "CloseBtn", new Vector2(60, 60));
            var cbrt = closeBtn.GetComponent<RectTransform>();
            cbrt.anchorMin = new Vector2(1, 0.5f);
            cbrt.anchorMax = new Vector2(1, 0.5f);
            cbrt.pivot = new Vector2(1, 0.5f);
            cbrt.anchoredPosition = new Vector2(-10, 0);
            AddImage(closeBtn, ButtonBg);
            AddButton(closeBtn);
            CreateText(closeBtn.transform, "CloseLabel", "X", 24, TextAlignmentOptions.Center);

            // Branch tabs
            var tabBar = CreateUIObject(panel.transform, "BranchTabs", Vector2.zero);
            var tbrt = tabBar.GetComponent<RectTransform>();
            tbrt.anchorMin = new Vector2(0, 1);
            tbrt.anchorMax = new Vector2(1, 1);
            tbrt.pivot = new Vector2(0.5f, 1);
            tbrt.anchoredPosition = new Vector2(0, -80);
            tbrt.sizeDelta = new Vector2(0, 50);

            var branchTabs = new Button[3];
            var branchTabLabels = new TextMeshProUGUI[3];
            string[] defaultBranchNames = { "Branch 1", "Branch 2", "Branch 3" };
            for (int i = 0; i < 3; i++)
            {
                float tabWidth = 1f / 3f;
                var tab = CreateUIObject(tabBar.transform, $"BranchTab_{i}", Vector2.zero);
                var trt = tab.GetComponent<RectTransform>();
                trt.anchorMin = new Vector2(tabWidth * i, 0);
                trt.anchorMax = new Vector2(tabWidth * (i + 1), 1);
                trt.offsetMin = new Vector2(2, 0);
                trt.offsetMax = new Vector2(-2, 0);

                AddImage(tab, i == 0 ? TabActive : TabInactive);
                branchTabs[i] = AddButton(tab);

                var label = CreateText(tab.transform, $"BranchLabel_{i}", defaultBranchNames[i], 20, TextAlignmentOptions.Center);
                var lrt = label.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
                branchTabLabels[i] = label.GetComponent<TextMeshProUGUI>();
            }

            // Tree container (ScrollRect)
            var scrollObj = CreateUIObject(panel.transform, "TreeScroll", Vector2.zero);
            var srt = scrollObj.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 0.3f);
            srt.anchorMax = new Vector2(1, 1);
            srt.offsetMin = new Vector2(10, 0);
            srt.offsetMax = new Vector2(-10, -135);

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            AddImage(scrollObj, new Color(0, 0, 0, 0.3f));

            var mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var treeContainer = CreateUIObject(scrollObj.transform, "TreeContainer", new Vector2(1000, 1000));
            var tcrt = treeContainer.GetComponent<RectTransform>();
            tcrt.anchorMin = new Vector2(0.5f, 0.5f);
            tcrt.anchorMax = new Vector2(0.5f, 0.5f);
            tcrt.pivot = new Vector2(0.5f, 0.5f);
            tcrt.anchoredPosition = Vector2.zero;

            scrollRect.content = tcrt;

            // Node detail panel (bottom)
            var nodeDetail = CreateUIObject(panel.transform, "NodeDetailPanel", Vector2.zero);
            var ndrt = nodeDetail.GetComponent<RectTransform>();
            ndrt.anchorMin = new Vector2(0, 0);
            ndrt.anchorMax = new Vector2(1, 0.3f);
            ndrt.offsetMin = new Vector2(10, 80); // above points bar
            ndrt.offsetMax = new Vector2(-10, -5);
            AddImage(nodeDetail, PanelBg);
            nodeDetail.SetActive(false);

            var nodeNameText = CreateText(nodeDetail.transform, "NodeNameText", "Node Name", 24, TextAlignmentOptions.TopLeft);
            var nnrt = nodeNameText.GetComponent<RectTransform>();
            nnrt.anchorMin = new Vector2(0, 0.6f);
            nnrt.anchorMax = new Vector2(0.6f, 1);
            nnrt.offsetMin = new Vector2(15, 5);
            nnrt.offsetMax = new Vector2(-5, -10);
            nodeNameText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            var nodeDescText = CreateText(nodeDetail.transform, "NodeDescText", "Description", 18, TextAlignmentOptions.TopLeft);
            var nddrt = nodeDescText.GetComponent<RectTransform>();
            nddrt.anchorMin = new Vector2(0, 0.2f);
            nddrt.anchorMax = new Vector2(0.6f, 0.6f);
            nddrt.offsetMin = new Vector2(15, 5);
            nddrt.offsetMax = new Vector2(-5, -5);

            var nodeCostText = CreateText(nodeDetail.transform, "NodeCostText", "Cost: 1 SP", 18, TextAlignmentOptions.TopLeft);
            var ncrt = nodeCostText.GetComponent<RectTransform>();
            ncrt.anchorMin = new Vector2(0, 0);
            ncrt.anchorMax = new Vector2(0.3f, 0.2f);
            ncrt.offsetMin = new Vector2(15, 5);
            ncrt.offsetMax = new Vector2(-5, -5);

            var nodeStatsText = CreateText(nodeDetail.transform, "NodeStatsText", "+5 ATK", 18, TextAlignmentOptions.TopLeft);
            var nsrt = nodeStatsText.GetComponent<RectTransform>();
            nsrt.anchorMin = new Vector2(0.6f, 0.3f);
            nsrt.anchorMax = new Vector2(1, 1);
            nsrt.offsetMin = new Vector2(10, 5);
            nsrt.offsetMax = new Vector2(-10, -10);

            // Invest button
            var investBtn = CreateUIObject(nodeDetail.transform, "InvestBtn", new Vector2(160, 50));
            var ibrt = investBtn.GetComponent<RectTransform>();
            ibrt.anchorMin = new Vector2(0.6f, 0);
            ibrt.anchorMax = new Vector2(0.6f, 0);
            ibrt.pivot = new Vector2(0, 0);
            ibrt.anchoredPosition = new Vector2(10, 10);
            AddImage(investBtn, TabActive);
            var investButton = AddButton(investBtn);

            var investLabel = CreateText(investBtn.transform, "InvestLabel", "Invest", 20, TextAlignmentOptions.Center);
            var ilrt = investLabel.GetComponent<RectTransform>();
            ilrt.anchorMin = Vector2.zero;
            ilrt.anchorMax = Vector2.one;
            ilrt.offsetMin = Vector2.zero;
            ilrt.offsetMax = Vector2.zero;

            // Points bar (bottom strip)
            var pointsBar = CreateUIObject(panel.transform, "PointsBar", Vector2.zero);
            var pbrt = pointsBar.GetComponent<RectTransform>();
            pbrt.anchorMin = new Vector2(0, 0);
            pbrt.anchorMax = new Vector2(1, 0);
            pbrt.pivot = new Vector2(0.5f, 0);
            pbrt.anchoredPosition = Vector2.zero;
            pbrt.sizeDelta = new Vector2(0, 70);
            AddImage(pointsBar, PanelBg);

            var pointsText = CreateText(pointsBar.transform, "PointsText", "SP: 0 available / 0 spent", 22, TextAlignmentOptions.MidlineLeft);
            var ptrt = pointsText.GetComponent<RectTransform>();
            ptrt.anchorMin = new Vector2(0, 0);
            ptrt.anchorMax = new Vector2(0.6f, 1);
            ptrt.offsetMin = new Vector2(20, 0);
            ptrt.offsetMax = new Vector2(-10, 0);

            // Respec button
            var respecBtn = CreateUIObject(pointsBar.transform, "RespecBtn", new Vector2(160, 45));
            var rbrt = respecBtn.GetComponent<RectTransform>();
            rbrt.anchorMin = new Vector2(1, 0.5f);
            rbrt.anchorMax = new Vector2(1, 0.5f);
            rbrt.pivot = new Vector2(1, 0.5f);
            rbrt.anchoredPosition = new Vector2(-15, 0);
            AddImage(respecBtn, ButtonBg);
            var respecButton = AddButton(respecBtn);

            var respecLabel = CreateText(respecBtn.transform, "RespecLabel", "Respec", 18, TextAlignmentOptions.Center);
            var rlrt = respecLabel.GetComponent<RectTransform>();
            rlrt.anchorMin = Vector2.zero;
            rlrt.anchorMax = Vector2.one;
            rlrt.offsetMin = Vector2.zero;
            rlrt.offsetMax = Vector2.zero;

            // Respec cost text
            var respecCostText = CreateText(pointsBar.transform, "RespecCostText", "", 16, TextAlignmentOptions.MidlineRight);
            var rcrt = respecCostText.GetComponent<RectTransform>();
            rcrt.anchorMin = new Vector2(0.6f, 0);
            rcrt.anchorMax = new Vector2(0.82f, 1);
            rcrt.offsetMin = new Vector2(5, 0);
            rcrt.offsetMax = new Vector2(-5, 0);

            // Add SkillTreeUI component and wire all references
            var skillTreeUI = panel.AddComponent<UI.SkillTreeUI>();
            var stSo = new SerializedObject(skillTreeUI);

            SetRef(stSo, "_skillTreePanel", panel);
            SetRef(stSo, "_treeContainer", tcrt);
            SetRef(stSo, "_nodeButtonPrefab", nodePrefab);
            SetRef(stSo, "_connectionLinePrefab", linePrefab);

            // Branch tabs
            var tabsProp = stSo.FindProperty("_branchTabs");
            if (tabsProp != null)
            {
                tabsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                    tabsProp.GetArrayElementAtIndex(i).objectReferenceValue = branchTabs[i];
            }

            var tabLabelsProp = stSo.FindProperty("_branchTabLabels");
            if (tabLabelsProp != null)
            {
                tabLabelsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                    tabLabelsProp.GetArrayElementAtIndex(i).objectReferenceValue = branchTabLabels[i];
            }

            SetRef(stSo, "_nodeDetailPanel", nodeDetail);
            SetRef(stSo, "_nodeNameText", nodeNameText.GetComponent<TextMeshProUGUI>());
            SetRef(stSo, "_nodeDescriptionText", nodeDescText.GetComponent<TextMeshProUGUI>());
            SetRef(stSo, "_nodeCostText", nodeCostText.GetComponent<TextMeshProUGUI>());
            SetRef(stSo, "_nodeStatsText", nodeStatsText.GetComponent<TextMeshProUGUI>());
            SetRef(stSo, "_investButton", investButton);
            SetRef(stSo, "_investButtonText", investLabel.GetComponent<TextMeshProUGUI>());
            SetRef(stSo, "_pointsText", pointsText.GetComponent<TextMeshProUGUI>());
            SetRef(stSo, "_respecButton", respecButton);
            SetRef(stSo, "_respecCostText", respecCostText.GetComponent<TextMeshProUGUI>());

            SetRef(stSo, "_closeButton", closeBtn.GetComponent<Button>());

            stSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skillTreeUI);
            Debug.Log("[SkillTreeSceneWiring] SkillTreePanel created and wired");
            return panel;
        }

        // ── D) Wire HeroesPanelController ────────────────────────────
        static void WireHeroesPanelController(Transform canvasTransform, GameObject skillTreePanel)
        {
            // Find HeroesPanel inside CombatCanvas
            var heroesPanel = FindInChildren(canvasTransform, "HeroesPanel");
            if (heroesPanel == null)
            {
                Debug.LogWarning("[SkillTreeSceneWiring] HeroesPanel not found");
                return;
            }

            var controller = heroesPanel.GetComponent<UI.HeroesPanelController>();
            if (controller == null)
                controller = heroesPanel.AddComponent<UI.HeroesPanelController>();

            var so = new SerializedObject(controller);

            // Find HeroDetail sub-panel
            var heroDetail = FindInChildren(heroesPanel.transform, "HeroDetail");
            SetRef(so, "_heroDetailPanel", heroDetail);

            // Find StatsText
            if (heroDetail != null)
            {
                var statsText = FindInChildren(heroDetail.transform, "StatsText");
                if (statsText != null)
                    SetRef(so, "_statsText", statsText.GetComponent<TextMeshProUGUI>());

                var backBtn = FindInChildren(heroDetail.transform, "BackBtn");
                if (backBtn != null)
                    SetRef(so, "_backButton", backBtn.GetComponent<Button>());

                var skillTreeBtn = FindInChildren(heroDetail.transform, "Btn_SkillTree");
                if (skillTreeBtn != null)
                    SetRef(so, "_skillTreeButton", skillTreeBtn.GetComponent<Button>());

                var equipBtn = FindInChildren(heroDetail.transform, "Btn_Equipment");
                if (equipBtn != null)
                    SetRef(so, "_equipmentButton", equipBtn.GetComponent<Button>());

                var ascensionBtn = FindInChildren(heroDetail.transform, "Btn_Ascension");
                if (ascensionBtn != null)
                    SetRef(so, "_ascensionButton", ascensionBtn.GetComponent<Button>());
            }

            // Wire SkillTreeUI reference
            var skillTreeUI = skillTreePanel.GetComponent<UI.SkillTreeUI>();
            if (skillTreeUI != null)
                SetRef(so, "_skillTreeUI", skillTreeUI);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
            Debug.Log("[SkillTreeSceneWiring] HeroesPanelController wired");
        }

        // ── E) Remove Coming Soon ────────────────────────────────────
        static void RemoveComingSoonLabel(Transform canvasTransform)
        {
            var heroesPanel = FindInChildren(canvasTransform, "HeroesPanel");
            if (heroesPanel == null) return;

            var heroDetail = FindInChildren(heroesPanel.transform, "HeroDetail");
            if (heroDetail == null) return;

            var comingSoon = FindInChildren(heroDetail.transform, "ComingSoon");
            if (comingSoon != null)
            {
                Object.DestroyImmediate(comingSoon);
                Debug.Log("[SkillTreeSceneWiring] Removed ComingSoon label from HeroDetail");
            }
        }

        // ── Helpers ──────────────────────────────────────────────────

        static TMP_FontAsset FindFont()
        {
            try
            {
                var f = TMP_Settings.defaultFontAsset;
                if (f != null) return f;
            }
            catch { }

            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (string guid in guids)
            {
                var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (f != null) return f;
            }
            return null;
        }

        static Canvas FindCombatCanvas()
        {
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (c.gameObject.name == "CombatCanvas") return c;
            return null;
        }

        static GameObject FindInChildren(Transform parent, string name)
        {
            // Breadth-first search
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.gameObject.name == name) return child.gameObject;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                var result = FindInChildren(parent.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        static void DestroyChild(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop != null)
                prop.objectReferenceValue = value;
            else
                Debug.LogWarning($"[SkillTreeSceneWiring] Property '{propName}' not found");
        }

        static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string[] parts = path.Replace("\\", "/").Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static GameObject CreateUIObject(Transform parent, string name, Vector2 sizeDelta)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<RectTransform>().sizeDelta = sizeDelta;
            return obj;
        }

        static Image AddImage(GameObject obj, Color color)
        {
            var img = obj.GetComponent<Image>();
            if (img == null) img = obj.AddComponent<Image>();
            img.color = color;
            return img;
        }

        static Button AddButton(GameObject obj)
        {
            var btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;
            return btn;
        }

        static GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
        {
            var obj = CreateUIObject(parent, name, new Vector2(400, 50));
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.richText = true;
            tmp.enableWordWrapping = true;

            if (_font != null)
            {
                var so = new SerializedObject(tmp);
                var fontProp = so.FindProperty("m_fontAsset");
                if (fontProp != null)
                    fontProp.objectReferenceValue = _font;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return obj;
        }
    }
}
#endif
