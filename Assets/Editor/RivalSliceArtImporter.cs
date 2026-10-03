using UnityEditor;
using UnityEngine;

namespace KindredSiege.EditorTools
{
    /// Pixel art under Resources/RivalSlice/Units imports crisp: no filtering, no compression,
    /// no mipmaps and no resizing, so a 64 px sprite stays 64 sharp pixels.
    public sealed class RivalSliceArtImporter : AssetPostprocessor
    {
        const string UnitsFolder = "Resources/RivalSlice/Units/";

        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains(UnitsFolder)) return;
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
