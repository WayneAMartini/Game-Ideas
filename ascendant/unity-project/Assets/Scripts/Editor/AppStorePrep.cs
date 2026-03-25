#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ascendant.Editor
{
    public static class AppStorePrep
    {
        [MenuItem("Ascendant/App Store/Configure iOS Build Settings")]
        public static void ConfigureiOSBuildSettings()
        {
            // Target iOS
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            // Architecture: arm64 only
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // 1 = ARM64

            // Minimum iOS version
            PlayerSettings.iOS.targetOSVersionString = "16.0";

            // Bundle identifier
            if (string.IsNullOrEmpty(PlayerSettings.applicationIdentifier))
                PlayerSettings.applicationIdentifier = "com.ascendant.herosimulatorrpg";

            // Display settings
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Rendering
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { UnityEngine.Rendering.GraphicsDeviceType.Metal });

            // Optimization
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Medium);

            // Status bar
            PlayerSettings.statusBarHidden = true;

            Debug.Log("[AppStorePrep] iOS build settings configured.");
        }

        [MenuItem("Ascendant/App Store/Set App Icon (Placeholder)")]
        public static void SetAppIcon()
        {
            // Generate if not exists
            SpriteGenerator.GenerateAll();
            Debug.Log("[AppStorePrep] Placeholder app icon generated. Set it in Player Settings > iOS > Icon.");
        }

        [MenuItem("Ascendant/App Store/Configure Launch Screen")]
        public static void ConfigureLaunchScreen()
        {
            // Unity iOS launch screen configuration
            PlayerSettings.iOS.showActivityIndicatorOnLoading = ActivityIndicatorStyle.WhiteLarge;

            // Set launch screen type to custom storyboard or default
            // For now, use Unity's built-in launch screen with background color
            Debug.Log("[AppStorePrep] Launch screen configured. Set custom storyboard in Xcode post-build if needed.");
        }

        [MenuItem("Ascendant/App Store/Log Age Rating Info")]
        public static void LogAgeRatingInfo()
        {
            Debug.Log("=== Ascendant Age Rating Configuration ===");
            Debug.Log("Suggested rating: 12+ (Teen)");
            Debug.Log("Reasons:");
            Debug.Log("  - Fantasy violence (combat with monsters)");
            Debug.Log("  - In-app purchases (gacha, battle pass, IAP)");
            Debug.Log("  - Simulated gambling (gacha/summon system)");
            Debug.Log("");
            Debug.Log("Required disclosures:");
            Debug.Log("  - Gacha rate disclosure screen (GachaRateDisclosure in SummonUI)");
            Debug.Log("  - Privacy Policy URL in Settings");
            Debug.Log("  - IDFA usage disclosure (for analytics)");
        }

        [MenuItem("Ascendant/App Store/Log Privacy Policy Requirements")]
        public static void LogPrivacyPolicyRequirements()
        {
            Debug.Log("=== Privacy Policy Requirements ===");
            Debug.Log("Data collected via Firebase:");
            Debug.Log("  - Anonymous user ID (Firebase Auth)");
            Debug.Log("  - Game progress and save data (Firestore)");
            Debug.Log("  - Crash reports (Crashlytics)");
            Debug.Log("  - Basic analytics events (Firebase Analytics)");
            Debug.Log("");
            Debug.Log("Data NOT collected:");
            Debug.Log("  - Personal information (name, email - unless linked)");
            Debug.Log("  - Location data");
            Debug.Log("  - Contact information");
            Debug.Log("");
            Debug.Log("Privacy Policy URL placeholder: https://ascendant-game.example.com/privacy");
        }
    }
}
#endif
