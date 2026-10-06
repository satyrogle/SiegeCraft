using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    /// Plays every idle sheet in Resources/RivalSlice/Sheets the way the Rival Slice does: four
    /// frames side by side, feet on the bottom edge, 64 pixels to a unit, one sprite per frame.
    /// For each tribe (and the chiefs) it lays the sheets out in two rows, facing right on top and
    /// facing left below, renders each of the four frames through a camera at one pixel per sheet
    /// pixel, and saves them as PNGs for a GIF at the game's idle speed (0.22 s a frame).
    ///
    /// Batch, from the project folder:
    ///   Unity.exe -batchmode -projectPath . -executeMethod KindredSiege.EditorTools.IdleSheetRecorder.Record -ksIdleOut <folder>
    public static class IdleSheetRecorder
    {
        public const string OutArg = "-ksIdleOut";
        const string SheetFolder = "Assets/Resources/RivalSlice/Sheets/";
        const float PixelsPerUnit = 64f;
        const int Frames = 4;
        const float Gap = 0.25f;

        static readonly string[] Classes = { "warden", "skirmisher", "trapper", "marksman", "medic" };

        public static void Record()
        {
            string outDir = ArgAfter(OutArg) ?? "Logs/IdleSheets";
            Directory.CreateDirectory(outDir);
            var log = new StringBuilder();
            bool ok = true;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var groups = new List<KeyValuePair<string, List<string>>>();
                foreach (string tribe in new[] { "clarth", "blood_rot", "skull" })
                {
                    var names = new List<string>();
                    foreach (string c in Classes) names.Add(tribe + "_" + c);
                    groups.Add(new KeyValuePair<string, List<string>>(tribe, names));
                }
                groups.Add(new KeyValuePair<string, List<string>>("chiefs",
                    new List<string> { "clarth_chief", "blood_rot_chief", "skull_chief" }));

                foreach (KeyValuePair<string, List<string>> group in groups)
                    ok &= RecordGroup(group.Key, group.Value, outDir, log);
            }
            catch (System.Exception e)
            {
                ok = false;
                log.AppendLine("ERROR " + e);
            }
            File.WriteAllText(Path.Combine(outDir, "result.txt"), (ok ? "PASS\n" : "FAIL\n") + log);
            Debug.Log("IdleSheetRecorder: " + (ok ? "PASS" : "FAIL") + "\n" + log);
            if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
        }

        /// One group: each unit's facing right sheet in the top row and facing left below.
        static bool RecordGroup(string groupName, List<string> units, string outDir, StringBuilder log)
        {
            var root = new GameObject("Idle " + groupName);
            var renderers = new List<SpriteRenderer>();
            var frames = new List<Sprite[]>();
            float cellWidth = 0f;
            float cellHeight = 0f;
            bool ok = true;

            // Load every sheet first, so the grid fits the largest one.
            var loaded = new List<KeyValuePair<string, Sprite[]>>();
            foreach (string facing in new[] { "se", "sw" })
            {
                foreach (string unit in units)
                {
                    string name = unit + "_idle_" + facing;
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetFolder + name + ".png");
                    if (texture == null)
                    {
                        log.AppendLine("MISSING " + name);
                        ok = false;
                        loaded.Add(new KeyValuePair<string, Sprite[]>(name, null));
                        continue;
                    }
                    int w = texture.width / Frames;
                    var sprites = new Sprite[Frames];
                    for (int i = 0; i < Frames; i++)
                        sprites[i] = Sprite.Create(texture, new Rect(i * w, 0f, w, texture.height), new Vector2(0.5f, 0f), PixelsPerUnit);
                    cellWidth = Mathf.Max(cellWidth, w / PixelsPerUnit);
                    cellHeight = Mathf.Max(cellHeight, texture.height / PixelsPerUnit);
                    log.AppendLine(name + ": " + texture.width + "x" + texture.height + ", filter " + texture.filterMode +
                        ", frame " + w + "x" + texture.height);
                    loaded.Add(new KeyValuePair<string, Sprite[]>(name, sprites));
                }
            }

            int columns = units.Count;
            Material spriteMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            for (int k = 0; k < loaded.Count; k++)
            {
                if (loaded[k].Value == null) continue;
                int row = k / columns;
                int col = k % columns;
                var go = new GameObject(loaded[k].Key);
                go.transform.SetParent(root.transform);
                go.transform.position = new Vector3((col + 0.5f) * (cellWidth + Gap), -(row + 1) * (cellHeight + Gap) + Gap, 0f);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                if (spriteMaterial != null) renderer.sharedMaterial = spriteMaterial;
                renderer.sprite = loaded[k].Value[0];
                renderers.Add(renderer);
                frames.Add(loaded[k].Value);
            }

            float width = columns * (cellWidth + Gap);
            float height = 2f * (cellHeight + Gap);
            var cameraObject = new GameObject("Idle camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = height * 0.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(58, 52, 48, 255);
            camera.transform.position = new Vector3(width * 0.5f, -height * 0.5f, -10f);
            int pixelWidth = Mathf.CeilToInt(width * PixelsPerUnit);
            int pixelHeight = Mathf.CeilToInt(height * PixelsPerUnit);
            camera.aspect = pixelWidth / (float)pixelHeight;
            var target = new RenderTexture(pixelWidth, pixelHeight, 24, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point };
            camera.targetTexture = target;
            var capture = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGBA32, false);

            for (int f = 0; f < Frames; f++)
            {
                for (int i = 0; i < renderers.Count; i++) renderers[i].sprite = frames[i][f];
                camera.Render();
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                capture.ReadPixels(new Rect(0, 0, pixelWidth, pixelHeight), 0, 0);
                capture.Apply();
                RenderTexture.active = previous;
                string path = Path.Combine(outDir, groupName + "_" + f + ".png");
                File.WriteAllBytes(path, capture.EncodeToPNG());
            }
            log.AppendLine(groupName + ": " + renderers.Count + " sheets, " + Frames + " frames at " + pixelWidth + "x" + pixelHeight +
                ", order " + string.Join(", ", units) + " (top facing right, bottom facing left)");

            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(capture);
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(root);
            return ok;
        }

        static string ArgAfter(string flag)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == flag) return args[i + 1];
            }
            return null;
        }
    }
}
