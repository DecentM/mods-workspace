using System.Collections;
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

        public void Compile()
        {
            DirectoryInfo info = new DirectoryInfo(this.sourcePath);
            FileInfo[] fileInfos = info.GetFiles();

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

                    default:
                        reader.Close();
                        break;
                        // throw new NotImplementedException($"Subtitle format {fileInfo.Extension} is not supported. Use a different file, or convert your subtitles to a supported format.");
                }

                reader.Close();

                // Don't write to a file if its source was ignored
                if (instructions.Count == 0)
                {
                    continue;
                }

                StreamWriter writer = new StreamWriter(Path.Combine(this.targetPath, $"{fileInfo.Name}.txt"));

                instructions.ForEach(delegate (Instruction instruction)
                {
                    writer.WriteLine(instruction.ToString());
                });

                writer.Close();
            }
        }
    }
}

