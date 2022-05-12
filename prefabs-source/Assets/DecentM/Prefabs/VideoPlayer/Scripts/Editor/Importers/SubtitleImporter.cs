using System;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Text;

using DecentM.EditorTools;
using DecentM.Icons;
using DecentM.Subtitles;

namespace DecentM.VideoPlayer.EditorTools.Importers
{
    [ScriptedImporter(
        1,
        new string[] { SubtitleFormat.Srt, SubtitleFormat.Vtt, SubtitleFormat.Vsi }
    )]
    public class SubtitleImporter : ScriptedImporter
    {
        private static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
                return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0)
                return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe)
                return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff)
                return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
                return new UTF32Encoding(true, true); //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            return Encoding.UTF8;
        }

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
