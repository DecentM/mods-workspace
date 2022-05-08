using System;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

using DecentM.EditorTools;

namespace DecentM.VideoPlayer.EditorTools.Importers
{
    [ScriptedImporter(1, ".ytdl-json")]
    public class MetadataImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!ctx.assetPath.Contains(EditorAssets.VideoMetadataFolder)) return;

            if (!File.Exists(ctx.assetPath))
            {
                ctx.LogImportError($"Imported file disappeared from {ctx.assetPath}");
                return;
            }

            string rawMetadata = File.ReadAllText(ctx.assetPath);
            VideoMetadataAsset asset = VideoMetadataAsset.CreateInstance(rawMetadata);

            ctx.AddObjectToAsset(Path.GetFileName(ctx.assetPath), asset, EditorAssets.SubtitlesOutlineIcon);
            ctx.SetMainObject(asset);
        }
    }
}
