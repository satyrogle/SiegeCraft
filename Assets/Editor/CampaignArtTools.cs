using System;
using System.IO;
using KindredSiege.Slice;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    public static class CampaignArtTools
    {
        public const string ScenePath="Assets/Scenes/Prototypes/CampaignArtLoop.unity";
        [MenuItem("KindredSiege/Campaign Art/Build Windows")]
        public static void Build()
        {
            bool success=false;
            try
            {
                if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
                AssetDatabase.Refresh();
                foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/CampaignArt"}))
                {
                    var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                    importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
                    importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                    importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;
                    importer.alphaIsTransparency=true;importer.SaveAndReimport();
                }
                if(!File.Exists(ScenePath))
                {
                    var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                    var camera=new GameObject("Main Camera",typeof(Camera));camera.tag="MainCamera";camera.GetComponent<Camera>().orthographic=true;
                    new GameObject("Campaign art loop",typeof(CampaignArtLoop));
                    EditorSceneManager.SaveScene(scene,ScenePath);
                }
                Directory.CreateDirectory("Builds/CampaignArt");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},
                    locationPathName="Builds/CampaignArt/KindredSiegeCampaign.exe",target=BuildTarget.StandaloneWindows64,
                    targetGroup=BuildTargetGroup.Standalone,options=BuildOptions.Development});
                success=report.summary.result==BuildResult.Succeeded;
                Debug.Log("[CampaignArtBuild] "+report.summary.result+" errors="+report.summary.totalErrors);
            }
            catch(Exception e){Debug.LogException(e);}
            if(Application.isBatchMode)EditorApplication.Exit(success?0:1);
        }
    }
}
