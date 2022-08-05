using System;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Text;

using DecentM.Icons;
using DecentM.Prefabs.Subtitles;

namespace DecentM.Prefabs.VideoPlayer.EditorTools.Importers
{
    [ScriptedImporter(
        1,
        new string[] { SubtitleFormat.Srt, SubtitleFormat.Vtt, SubtitleFormat.Vsi }
    )]
    public class SubtitleImporter : ScriptedImporter
    {
        private static Encoding DetectFileEncoding(string filename)
        {
            Stream fs = File.OpenRead(filename);
            var Utf8EncodingVerifier = Encoding.GetEncoding(
                "utf-8",
                new EncoderExceptionFallback(),
                new DecoderExceptionFallback()
            );
            using (
                var reader = new StreamReader(
                    fs,
                    Utf8EncodingVerifier,
                    true,
                    leaveOpen: true,
                    bufferSize: 1024
                )
            )
            {
                string detectedEncoding;
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                    }
                    detectedEncoding = reader.CurrentEncoding.BodyName;
                }
                catch
                {
                    // Failed to decode the file using the BOM/UT8.
                    // Assume it's local ANSI
                    detectedEncoding = "ISO-8859-1";
                }
                // Rewind the stream
                fs.Seek(0, SeekOrigin.Begin);
                fs.Close();
                return Encoding.GetEncoding(detectedEncoding);
            }
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!File.Exists(ctx.assetPath))
            {
                ctx.LogImportError($"Imported file disappeared from {ctx.assetPath}");
                TextAsset errorAsset = new TextAsset("");
                ctx.AddObjectToAsset(
                    Path.GetFileName(ctx.assetPath),
                    errorAsset,
                    MaterialIcons.GetIcon(Icon.Close)
                );
                ctx.SetMainObject(errorAsset);
                return;
            }

            Encoding encoding = DetectFileEncoding(ctx.assetPath);
            byte[] fileBytes = File.ReadAllBytes(ctx.assetPath);
            byte[] encodedBytes = Encoding.Convert(encoding, Encoding.UTF8, fileBytes);

            string source = Encoding.UTF8.GetString(encodedBytes);
            Compiler.CompilationResult compiled = SubtitleCompiler.Compile(
                source,
                Path.GetExtension(ctx.assetPath),
                SubtitleFormat.Vsi
            );
            TextAsset asset = new TextAsset(compiled.output);

            if (compiled.errors.Count > 0)
            {
                foreach (CompilationResultError error in compiled.errors)
                {
                    ctx.LogImportWarning(error.value);
                }

                ctx.LogImportWarning(
                    $"{compiled.errors.Count} error(s) encountered while compiling subtitle file, see above. Continuing with possibly incomplete compilation results..."
                );
            }

            ctx.AddObjectToAsset(
                Path.GetFileName(ctx.assetPath),
                asset,
                MaterialIcons.GetIcon(Icon.SubtitlesOutline)
            );
            ctx.SetMainObject(asset);
        }
    }
}
