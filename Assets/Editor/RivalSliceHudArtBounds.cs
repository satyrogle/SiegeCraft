using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    // Bake opaque-pixel tops without enabling Read/Write on the shipping textures.
    // One maximum across the idle frames keeps labels steady while a fighter breathes.
    public sealed class RivalSliceHudArtBounds : AssetPostprocessor
    {
        const string Folder = "Assets/Resources/RivalSlice/Sheets/";
        const string Output = "Assets/Resources/RivalSlice/HudArtBounds.json";
        static bool pending;
        [Serializable] sealed class Entry { public string name; public int top; }
        [Serializable] sealed class BoundsFile { public List<Entry> entries = new List<Entry>(); }

        static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved,string[] movedFrom)
        {
            bool changed=false;
            foreach(var paths in new[]{imported,deleted,moved,movedFrom})
                foreach(string path in paths) if(path.StartsWith(Folder,StringComparison.Ordinal)&&path.EndsWith(".png",StringComparison.OrdinalIgnoreCase))changed=true;
            if(!changed||pending)return;
            pending=true;
            EditorApplication.delayCall+=()=>{pending=false;Rebuild();};
        }

        [MenuItem("Kindred Siege/Art/Rebuild HUD sprite bounds")]
        public static void Rebuild()
        {
            if(!Directory.Exists(Folder))return;
            var file=new BoundsFile();var paths=Directory.GetFiles(Folder,"*.png");Array.Sort(paths,StringComparer.Ordinal);
            foreach(string path in paths)
            {
                var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
                try
                {
                    if(!texture.LoadImage(File.ReadAllBytes(path)))continue;
                    Color32[] pixels=texture.GetPixels32();int top=0;
                    for(int y=texture.height-1;y>=0&&top==0;y--)
                        for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>0){top=y+1;break;}
                    file.entries.Add(new Entry{name=Path.GetFileNameWithoutExtension(path),top=top});
                }
                finally { UnityEngine.Object.DestroyImmediate(texture); }
            }
            string json=JsonUtility.ToJson(file,true)+"\n";
            if(File.Exists(Output)&&File.ReadAllText(Output)==json)return;
            File.WriteAllText(Output,json);AssetDatabase.ImportAsset(Output);
        }
    }
}
