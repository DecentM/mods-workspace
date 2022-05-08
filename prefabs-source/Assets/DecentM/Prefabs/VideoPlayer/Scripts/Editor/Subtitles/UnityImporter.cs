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
        private string targetPath = "Assets/DecentM/Assets/CompiledSubtitleInstructions";
        private string sourcePath = "Assets/DecentM/Assets/Subtitles";

        public List<TextAsset> ImportAll()
        {
            DirectoryInfo info = new DirectoryInfo(this.sourcePath);

            List<FileInfo> fileInfos = info.GetFiles().Where(file => SubtitleFormat.IsSupported(file.Extension)).ToList();
            List<TextAsset> assets = new List<TextAsset>();

            // Go through all the files in the directory and parse them using a parser based on its file extension
            for (int i = 0; i < fileInfos.Count; i++)
            {
                FileInfo fileInfo = fileInfos[i];
                EditorUtility.DisplayProgressBar($"Processing subtitles... ({i}/{fileInfos.Count})", fileInfo.Name, (float)i / fileInfos.Count);
                StreamReader reader = new StreamReader(fileInfo.FullName);

                Debug.Log(fileInfo.Name);

                try
                {
                    SubtitleCompiler.CompilationResult result = SubtitleCompiler.Compile(reader.ReadToEnd(), fileInfo.Extension);
                    reader.Close();

                    if (result.errors.Count != 0)
                    {
                        throw new Exception($"Encountered {result.errors.Count} errors while parsing {fileInfo.Name}, some sections might be missing from the output.");
                    }

                    TextAsset asset = new TextAsset(result.output);
                    AssetDatabase.CreateAsset(asset, Path.Combine(this.targetPath, $"{fileInfo.Name}.asset"));

                    assets.Add(asset);
                } catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }

            EditorUtility.ClearProgressBar();

            return assets;
        }
    }
}
#endif
