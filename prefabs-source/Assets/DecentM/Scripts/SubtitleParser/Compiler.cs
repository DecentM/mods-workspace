using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

namespace DecentM.Subtitles
{
    class FileTypes
    {
        public const string Srt = ".srt";
        public const string Ass = ".ass";
        public const string Usf = ".usf";
        public const string Vtt = ".vtt";
        public const string Stl = ".stl";
        public const string Sub = ".sub";
        public const string Ssa = ".ssa";
        public const string Ttxt = ".ttxt";

        public static bool IsSupported(string filetype)
        {
            return filetype == Srt;
        }
    }

    public class Compiler
    {
        private Srt.Parser srtParser = new Srt.Parser();
        private string targetPath = "Assets/DecentM/Assets/CompiledSubtitleInstructions";
        private string sourcePath = "Assets/DecentM/Assets/Subtitles";

        public List<TextAsset> Compile()
        {
            DirectoryInfo info = new DirectoryInfo(this.sourcePath);

            FileInfo[] fileInfos = info.GetFiles().Where(file => FileTypes.IsSupported(file.Extension)).ToArray();
            List<TextAsset> assets = new List<TextAsset>();

            // Go through all the files in the directory and parse them using a parser based on its file extension
            for (int i = 0; i < fileInfos.Length; i++)
            {
                FileInfo fileInfo = fileInfos[i];

#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar($"Processing subtitles... ({i}/{fileInfos.Length})", fileInfo.Name, (float)i / fileInfos.Length);
#endif

                StreamReader reader = new StreamReader(fileInfo.FullName);
                List<Instruction> instructions = new List<Instruction>();

                switch (fileInfo.Extension)
                {
                    case FileTypes.Srt:
                        instructions = this.srtParser.Parse(reader.ReadToEnd());
                        break;

                    case FileTypes.Ass:
                    case FileTypes.Usf:
                    case FileTypes.Vtt:
                    case FileTypes.Stl:
                    case FileTypes.Sub:
                    case FileTypes.Ssa:
                    case FileTypes.Ttxt:
                        reader.Close();
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

#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif

            return assets;
        }
    }
}

