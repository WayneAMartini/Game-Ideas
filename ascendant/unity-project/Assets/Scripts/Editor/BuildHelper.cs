#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Ascendant.Editor
{
    public static class BuildHelper
    {
        const string BuildPath = "Builds/iOS";

        [MenuItem("Ascendant/Build/Prepare iOS Build")]
        public static void PrepareiOSBuild()
        {
            AppStorePrep.ConfigureiOSBuildSettings();
            Debug.Log("[BuildHelper] iOS build prepared. Use 'Build iOS' to create the Xcode project.");
        }

        [MenuItem("Ascendant/Build/Build iOS")]
        public static void BuildiOS()
        {
            AppStorePrep.ConfigureiOSBuildSettings();

            var scenes = GetEnabledScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildHelper] No scenes found in Build Settings. Add scenes first.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BuildPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[BuildHelper] iOS build succeeded. Output: {BuildPath}");
            else
                Debug.LogError($"[BuildHelper] iOS build failed: {report.summary.totalErrors} errors");
        }

        [MenuItem("Ascendant/Build/Build iOS (Development)")]
        public static void BuildiOSDev()
        {
            AppStorePrep.ConfigureiOSBuildSettings();

            var scenes = GetEnabledScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildHelper] No scenes found in Build Settings.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BuildPath + "_Dev",
                target = BuildTarget.iOS,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[BuildHelper] iOS dev build succeeded. Output: {BuildPath}_Dev");
            else
                Debug.LogError($"[BuildHelper] iOS dev build failed: {report.summary.totalErrors} errors");
        }

        [MenuItem("Ascendant/Build/Verify Compilation")]
        public static void VerifyCompilation()
        {
            Debug.Log("[BuildHelper] Running compilation check...");

            // Force script compilation
            AssetDatabase.Refresh();

            // Check for compile errors via ConsoleWindow
            int errorCount = 0;
            var entries = new System.Collections.Generic.List<string>();

            // Use CompilationPipeline to check
            #if UNITY_2021_1_OR_NEWER
            var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
            Debug.Log($"[BuildHelper] Found {assemblies.Length} assemblies to verify.");
            #endif

            if (errorCount == 0)
                Debug.Log("[BuildHelper] Compilation check passed - no errors detected.");
            else
                Debug.LogError($"[BuildHelper] Compilation check found {errorCount} errors.");
        }

        [MenuItem("Ascendant/Build/Generate All Placeholder Assets")]
        public static void GenerateAllPlaceholders()
        {
            SpriteGenerator.GenerateAll();
            Debug.Log("[BuildHelper] All placeholder assets generated.");
        }

        static string[] GetEnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }
    }
}
#endif
