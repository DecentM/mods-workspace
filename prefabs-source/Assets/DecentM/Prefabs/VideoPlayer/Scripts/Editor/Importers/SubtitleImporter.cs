using System;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

using DecentM.EditorTools;
using DecentM.Icons;
using DecentM.Subtitles;

namespace DecentM.VideoPlayer.EditorTools.Importers
{
    [ScriptedImporter(1, new string[] { SubtitleFormat.Srt, SubtitleFormat.Vtt })]
    public class SrtImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!ctx.assetPath.Contains(EditorAssets.SubtitleCacheFolder)) return;

            if (!File.Exists(ctx.assetPath))
            {
                ctx.LogImportError($"Imported file disappeared from {ctx.assetPath}");
                TextAsset errorAsset = new TextAsset("");
                ctx.AddObjectToAsset(Path.GetFileName(ctx.assetPath), errorAsset, MaterialIcons.GetIcon(Icon.Close));
                ctx.SetMainObject(errorAsset);
                return;
            }

            string srt = File.ReadAllText(ctx.assetPath);
            SubtitleCompiler.CompilationResult compiled = SubtitleCompiler.Compile(srt, Path.GetExtension(ctx.assetPath));
            TextAsset asset = new TextAsset(compiled.output);
            asset.name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(ctx.assetPath));

            if (compiled.errors.Count > 0)
            {
                foreach (SubtitleCompiler.CompilationResultError error in compiled.errors)
                {
                    ctx.LogImportWarning(error.value);
                }

                ctx.LogImportWarning($"{compiled.errors.Count} error(s) encountered while compiling subtitle file, see above. Continuing with possibly incomplete compilation results...");
            }

            ctx.AddObjectToAsset(Path.GetFileName(ctx.assetPath), asset, MaterialIcons.GetIcon(Icon.SubtitlesOutline));
            ctx.SetMainObject(asset);
        }
    }
}
