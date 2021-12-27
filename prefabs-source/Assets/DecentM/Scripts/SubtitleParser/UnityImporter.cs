#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

namespace DecentM.Subtitles
{
    public class UnityImporter
    {
        private Compiler compiler = new Compiler();
        private string targetPath = "Assets/DecentM/Assets/CompiledSubtitleInstructions";
        private string sourcePath = "Assets/DecentM/Assets/Subtitles";

        public List<TextAsset> ImportAll()
        {
            DirectoryInfo info = new DirectoryInfo(this.sourcePath);

            FileInfo[] fileInfos = info.GetFiles().Where(file => FileTypes.IsSupported(file.Extension)).ToArray();
            List<TextAsset> assets = new List<TextAsset>();

            // Go through all the files in the directory and parse them using a parser based on its file extension
            for (int i = 0; i < fileInfos.Length; i++)
            {
                FileInfo fileInfo = fileInfos[i];

                EditorUtility.DisplayProgressBar($"Processing subtitles... ({i}/{fileInfos.Length})", fileInfo.Name, (float)i / fileInfos.Length);

                StreamReader reader = new StreamReader(fileInfo.FullName);

                try
                {
                    Compiler.CompilationResult result = this.compiler.Compile(reader.ReadToEnd(), fileInfo.Extension);

                    if (result.errors.Count != 0)
                    {
                        throw new Exception($"Encountered {result.errors.Count} errors while parsing subtitles.");
                    }

                    TextAsset asset = new TextAsset(result.output);
                    AssetDatabase.CreateAsset(asset, Path.Combine(this.targetPath, $"{fileInfo.Name}.asset"));

                    assets.Add(asset);
                } catch (Exception ex)
                {
                    if (!EditorUtility.DisplayDialog("Compilation error", $"This subtitle file has an error:\n{ex.Message}", "Continue", "Abort"))
                    {
                        break;
                    };
                }
            }

            EditorUtility.ClearProgressBar();

            return assets;
        }
    }
}
#endif
