using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace KindredSiege.EditorTools
{
    public static class CampaignArtSprintBuild
    {
        public static void Build()
        {
            bool success=false;
            try
            {
                string output="Builds/CampaignArtSprint-20261012";
                var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-ksBuildOutput")output=args[i+1];
                Directory.CreateDirectory(output);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{CampaignArtTools.ScenePath},
                    locationPathName=output+"/KindredSiegeCampaign.exe",target=BuildTarget.StandaloneWindows64,
                    targetGroup=BuildTargetGroup.Standalone,options=BuildOptions.None});
                success=report.summary.result==BuildResult.Succeeded;
                Debug.Log("[SprintBuild] "+report.summary.result+" errors="+report.summary.totalErrors);
            }
            catch(Exception e){Debug.LogException(e);}
            if(Application.isBatchMode)EditorApplication.Exit(success?0:1);
        }
    }
}
