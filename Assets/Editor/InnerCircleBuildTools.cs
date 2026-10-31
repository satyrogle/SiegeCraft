using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    public static class InnerCircleBuildTools
    {
        public static void Build()
        {
            bool success = false;
            try
            {
                AssetDatabase.Refresh();
                foreach (string name in new[] { "leader", "butcher", "glutton", "shade", "whisperer" })
                {
                    string path = "Assets/Resources/CampaignArt/InnerCircle/" + name + "_front.png";
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) throw new InvalidOperationException("Missing sprite: " + path);
                    importer.textureType = TextureImporterType.Default;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.maxTextureSize = 256;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (texture.width != 192 || texture.height != 192) throw new InvalidOperationException("Sprite is not 192px: " + path);
                }
                const string folder = "Builds/CampaignArt-InnerCircle";
                Directory.CreateDirectory(folder);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { CampaignArtTools.ScenePath },
                    locationPathName = folder + "/KindredSiegeCampaign.exe",
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.Development
                });
                success = report.summary.result == BuildResult.Succeeded;
                Debug.Log("[InnerCircleBuild] " + report.summary.result + " errors=" + report.summary.totalErrors);
            }
            catch (Exception e) { Debug.LogException(e); }
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
