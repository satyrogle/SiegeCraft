using UnityEditor;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    /// Pixel art under Resources/RivalSlice/Units, Sheets and Chiefs imports crisp: no filtering,
    /// no compression, no mipmaps and no resizing, so a 64 px sprite stays 64 sharp pixels. The
    /// tiling ground texture is left alone.
    public sealed class RivalSliceArtImporter : AssetPostprocessor
    {
        static readonly string[] PixelFolders =
        {
            "Resources/RivalSlice/Units/", "Resources/RivalSlice/Sheets/", "Resources/RivalSlice/Chiefs/"
        };

        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            bool pixelArt = false;
            foreach (string folder in PixelFolders) pixelArt |= path.Contains(folder);
            if (!pixelArt) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
