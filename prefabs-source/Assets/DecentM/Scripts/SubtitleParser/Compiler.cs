using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace DecentM.Subtitles
{
    public enum FileType
    {
        Srt,
    }

    public class Compiler
    {
        private Srt.Parser srtParser = new Srt.Parser();
        private string targetPath = "Assets/DecentM/Assets/CompiledSubtitleInstructions";
        private string sourcePath = "Assets/DecentM/Assets/Subtitles";

        public List<TextAsset> Compile()
        {
            DirectoryInfo info = new DirectoryInfo(this.sourcePath);
            FileInfo[] fileInfos = info.GetFiles();
            List<TextAsset> assets = new List<TextAsset>();

            // Go through all the files in the directory and parse them using a parser based on its file extension
            for (int i = 0; i < fileInfos.Length; i++)
            {
                FileInfo fileInfo = fileInfos[i];
                StreamReader reader = new StreamReader(fileInfo.FullName);
                List<Instruction> instructions = new List<Instruction>();

                switch (fileInfo.Extension)
                {
                    case ".srt":
                        instructions = this.srtParser.Parse(reader.ReadToEnd());
                        break;

                    case ".ass":
                    case ".usf":
                    case ".vtt":
                    case ".stl":
                    case ".sub":
                    case ".ssa":
                    case ".ttxt":
                        throw new NotImplementedException($"Subtitle format {fileInfo.Extension} is not supported. Use a different file, or convert your subtitles to a supported format.");

                    default:
                        reader.Close();
                        break;
                }

                reader.Close();

                // Don't write to a file if its source was ignored
                if (instructions.Count == 0)
                {
                    continue;
                }

                string text = "";

                instructions.ForEach(delegate (Instruction instruction)
                {
                    // writer.WriteLine(instruction.ToString());
                    text += instruction.ToString();
                    text += '\n';
                });

                TextAsset asset = new TextAsset(text);
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(asset, Path.Combine(this.targetPath, $"{fileInfo.Name}.asset"));
#endif

                assets.Add(asset);
            }

            return assets;
        }
    }
}

