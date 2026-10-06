using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    /// Builds the rival slice as a Windows player, to check it starts outside the editor:
    /// run Unity with -batchmode -quit -executeMethod KindredSiege.EditorTools.RivalSliceBuild.Build
    /// and optionally -ksBuildOut <folder>. The default folder, Builds/RivalSlice, is ignored by git.
    /// The editor exits with 0 when the build succeeds and 1 when it fails.
    public static class RivalSliceBuild
    {
        const string DefaultFolder = "Builds/RivalSlice";

        [MenuItem("KindredSiege/Prototypes/Build Rival Slice (Windows)")]
        public static void Build()
        {
            string folder = Argument("-ksBuildOut") ?? DefaultFolder;
            var options = new BuildPlayerOptions
            {
                scenes = new[] { RivalSliceRecorder.ScenePath },
                locationPathName = Path.Combine(folder, "RivalSlice.exe"),
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.Development
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            bool succeeded = summary.result == BuildResult.Succeeded;
            Debug.Log("[RivalSliceBuild] " + summary.result + ": " + summary.outputPath + ", " + summary.totalErrors + " errors, " +
                summary.totalWarnings + " warnings, " + summary.totalSize + " bytes, " + summary.totalTime);
            if (Application.isBatchMode) EditorApplication.Exit(succeeded ? 0 : 1);
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
