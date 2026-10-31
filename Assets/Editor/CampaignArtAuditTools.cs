using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace KindredSiege.EditorTools
{
    public static class CampaignArtAuditTools
    {
        // Build to a separate explicit output. Preserve the shipping package and current scene.
        public static void Build()
        {
            int exit=1;
            try
            {
                string[] args=Environment.GetCommandLineArgs();
                int i=Array.IndexOf(args,"-ksAuditBuild");
                if(i<0||i+1>=args.Length||!Path.IsPathRooted(args[i+1]))throw new ArgumentException("-ksAuditBuild requires an absolute .exe output path.");
                string output=Path.GetFullPath(args[i+1]);
                if(!output.EndsWith(".exe",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Audit output must end with .exe.");
                string assets=Path.GetFullPath("Assets")+Path.DirectorySeparatorChar;
                if(output.StartsWith(assets,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Build output must be outside Assets.");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes=new[]{CampaignArtTools.ScenePath},locationPathName=output,
                    target=BuildTarget.StandaloneWindows64,targetGroup=BuildTargetGroup.Standalone,
                    options=BuildOptions.Development
                });
                UnityEngine.Debug.Log("[CampaignArtAuditBuild] "+result.summary.result+" errors="+result.summary.totalErrors);
                if(result.summary.result==BuildResult.Succeeded)exit=0;
            }
            catch(Exception e){UnityEngine.Debug.LogException(e);}
            EditorApplication.Exit(exit);
        }
    }
}
