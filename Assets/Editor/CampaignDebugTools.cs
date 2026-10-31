using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using KindredSiege.CampaignDebug;

namespace KindredSiege.EditorTools
{
    /// The loop test (Jacob's go, 9 Oct 2026): plain debug screens over the campaign, separate from
    /// the rival slice. Opens or makes its scene, and builds it as a Windows player: run Unity with
    /// -batchmode -quit -executeMethod KindredSiege.EditorTools.CampaignDebugTools.Build and
    /// optionally -ksBuildOut folder. The default folder, Builds/CampaignDebug, is ignored by git.
    /// The editor exits with 0 when the build succeeds and 1 when it fails.
    public static class CampaignDebugTools
    {
        public const string ScenePath = "Assets/Scenes/Prototypes/CampaignDebug.unity";
        const string DefaultFolder = "Builds/CampaignDebug";
        const string ExeName = "KindredSiegeLoopTest.exe";

        [MenuItem("KindredSiege/Campaign Debug/Open Loop Test Scene")]
        public static void OpenScene()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode before opening the loop test."); return; }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            MakeSceneIfMissing();
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("KindredSiege/Campaign Debug/Build Loop Test (Windows)")]
        public static void Build()
        {
            bool succeeded = false;
            try
            {
                MakeSceneIfMissing();
                string folder = Argument("-ksBuildOut") ?? DefaultFolder;
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = Path.Combine(folder, ExeName),
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.Development
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;
                succeeded = summary.result == BuildResult.Succeeded;
                Debug.Log("[CampaignDebugTools] " + summary.result + ": " + summary.outputPath + ", " + summary.totalErrors + " errors, " +
                    summary.totalWarnings + " warnings, " + summary.totalSize + " bytes, " + summary.totalTime);
            }
            catch (Exception e)
            {
                // An exception in -executeMethod still exits 0 under -quit, so say so and fail.
                Debug.LogError("[CampaignDebugTools] Build failed: " + e);
            }
            if (Application.isBatchMode) EditorApplication.Exit(succeeded ? 0 : 1);
        }

        /// Makes the scene beside whatever is open, so the open scene is left alone. Unity refuses a
        /// new scene beside an untitled one, which is always the case in batch mode, so there the new
        /// scene replaces it, and in the editor the untitled scene is offered for saving first.
        static void MakeSceneIfMissing()
        {
            if (File.Exists(ScenePath)) return;
            if (!AssetDatabase.IsValidFolder("Assets/Scenes/Prototypes")) AssetDatabase.CreateFolder("Assets/Scenes", "Prototypes");
            bool additive = !Application.isBatchMode && !string.IsNullOrEmpty(SceneManager.GetActiveScene().path);
            if (!additive && !Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, additive ? NewSceneMode.Additive : NewSceneMode.Single);
            var cameraObject = new GameObject("Camera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.11f, 0.11f, 0.12f);
            var screen = new GameObject("Loop test", typeof(CampaignDebugScreen));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            SceneManager.MoveGameObjectToScene(screen, scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            if (SceneManager.sceneCount > 1) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
        }

        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.Ordinal)) return args[i + 1];
            }
            return null;
        }
    }
}
