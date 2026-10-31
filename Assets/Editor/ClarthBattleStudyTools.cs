using System;
using System.IO;
using KindredSiege.Slice;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    public static class ClarthBattleStudyTools
    {
        public const string ScenePath = "Assets/Scenes/Prototypes/ClarthBattleStudy.unity";

        [MenuItem("KindredSiege/Clarth Study/Open")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) MakeScene();
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("KindredSiege/Clarth Study/Build Windows")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !File.Exists(ScenePath) &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            bool success = false;
            try
            {
                AssetDatabase.Refresh();
                foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/ClarthStudy" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    importer.textureType = TextureImporterType.Default;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.maxTextureSize = 4096;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
                if (!File.Exists(ScenePath)) MakeScene();
                Directory.CreateDirectory("Builds/ClarthStudy");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath }, locationPathName = "Builds/ClarthStudy/KindredSiegeClarth.exe",
                    target = BuildTarget.StandaloneWindows64, targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.Development
                });
                success = report.summary.result == BuildResult.Succeeded;
                Debug.Log("[ClarthStudyBuild] " + report.summary.result + " errors=" + report.summary.totalErrors);
            }
            catch (Exception e) { Debug.LogException(e); }
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
        }

        static void MakeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera)); camera.tag = "MainCamera";
            camera.GetComponent<Camera>().orthographic = true;
            new GameObject("Clarth battle study", typeof(ClarthBattleStudy));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }
    }
}
