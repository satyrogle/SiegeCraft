using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    public static class CampaignArtFeedbackBuild
    {
        public static void Build()
        {
            bool success=false;
            try
            {
                const string output="Builds/CampaignArtFeedbackRepair-20261012-r4";
                Directory.CreateDirectory(output);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes=new[]{CampaignArtTools.ScenePath},
                    locationPathName=output+"/KindredSiegeCampaign.exe",
                    target=BuildTarget.StandaloneWindows64,targetGroup=BuildTargetGroup.Standalone,
                    options=BuildOptions.None
                });
                success=report.summary.result==BuildResult.Succeeded;
                Debug.Log("[CombatFeedbackBuild] "+report.summary.result+" errors="+report.summary.totalErrors);
            }
            catch(Exception e){Debug.LogException(e);}
            if(Application.isBatchMode)EditorApplication.Exit(success?0:1);
        }
    }
}
