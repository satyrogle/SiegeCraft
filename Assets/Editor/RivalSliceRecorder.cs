using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using KindredSiege.Slice;

namespace KindredSiege.EditorTools
{
    /// Opens the rival slice scene, and records it: run Unity with
    /// -executeMethod KindredSiege.EditorTools.RivalSliceRecorder.Record -ksRivalRecord <folder>
    /// The scene plays on autopilot, saves a frame per game frame into the folder, writes result.txt
    /// and the editor exits with 0 when the last card has shown.
    [InitializeOnLoad]
    public static class RivalSliceRecorder
    {
        public const string ScenePath = "Assets/Scenes/Prototypes/RivalSlice.unity";
        const string RecordKey = "KS.RivalSlice.Record";
        const string FolderKey = "KS.RivalSlice.Folder";
        const string StartedKey = "KS.RivalSlice.Started";
        const string ExitKey = "KS.RivalSlice.Exit";
        const double TimeoutSeconds = 900;

        static RivalSliceRecorder()
        {
            EditorApplication.playModeStateChanged += PlayState;
            if (SessionState.GetBool(RecordKey, false)) EditorApplication.update += Watch;
        }

        [MenuItem("KindredSiege/Prototypes/Open Rival Slice")]
        public static void OpenScene()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode before opening the rival slice."); return; }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            if (!AssetDatabase.IsValidFolder("Assets/Scenes/Prototypes")) AssetDatabase.CreateFolder("Assets/Scenes", "Prototypes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Rival slice", typeof(RivalSliceView));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        public static void Record()
        {
            string folder = string.Empty;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == RivalSliceView.RecordArg) folder = args[i + 1];
            }
            if (folder.Length == 0)
            {
                Debug.LogError("Record needs " + RivalSliceView.RecordArg + " <folder> on the command line.");
                EditorApplication.Exit(1);
                return;
            }

            Directory.CreateDirectory(folder);
            foreach (string old in Directory.GetFiles(folder, "frame_*.png")) File.Delete(old);
            File.WriteAllText(Path.Combine(folder, "result.txt"), "RUNNING\n");

            OpenScene();
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
            {
                File.WriteAllText(Path.Combine(folder, "result.txt"), "FAIL: the rival slice scene did not open\n");
                EditorApplication.Exit(1);
                return;
            }
            SessionState.SetBool(RecordKey, true);
            SessionState.SetString(FolderKey, folder);
            SessionState.SetInt(ExitKey, 1);
            EditorApplication.EnterPlaymode();
        }

        static void PlayState(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(RecordKey, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetFloat(StartedKey, (float)EditorApplication.timeSinceStartup);
                EditorApplication.update -= Watch;
                EditorApplication.update += Watch;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Watch;
                SessionState.SetBool(RecordKey, false);
                EditorApplication.Exit(SessionState.GetInt(ExitKey, 1));
            }
        }

        static void Watch()
        {
            if (!EditorApplication.isPlaying) return;
            var view = Object.FindObjectOfType<RivalSliceView>();
            bool finished = view != null && view.Finished;
            bool timedOut = EditorApplication.timeSinceStartup - SessionState.GetFloat(StartedKey, 0f) > TimeoutSeconds;
            if (!finished && !timedOut) return;

            string folder = SessionState.GetString(FolderKey, string.Empty);
            if (folder.Length > 0)
            {
                File.WriteAllText(Path.Combine(folder, "result.txt"), finished
                    ? "PASS\n" + view.Summary
                    : "FAIL: timed out at stage " + (view == null ? "none" : view.CurrentStage.ToString()) + "\n");
            }
            SessionState.SetInt(ExitKey, finished ? 0 : 1);
            EditorApplication.update -= Watch;
            EditorApplication.ExitPlaymode();
        }
    }
}
