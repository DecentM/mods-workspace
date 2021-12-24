using System.Collections.Generic;
using System;

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

        public static string[] SupportedFormats = { Srt };
    }

    public class Compiler
    {
        private Srt.Parser srtParser = new Srt.Parser();

        public string Compile(string source, string fileType)
        {
            List<Instruction> instructions = new List<Instruction>();

            if (!FileTypes.IsSupported(fileType))
            {
                throw new NotImplementedException($"Subtitle format {fileType} is not supported. Use a different file, or convert your subtitles to a supported format: {String.Join(", ", FileTypes.SupportedFormats)}");
            }

            switch (fileType)
            {
                case FileTypes.Srt:
                    instructions = this.srtParser.Parse(source);
                    break;

                case FileTypes.Ass:
                case FileTypes.Usf:
                case FileTypes.Vtt:
                case FileTypes.Stl:
                case FileTypes.Sub:
                case FileTypes.Ssa:
                case FileTypes.Ttxt:
                default:
                    break;
            }

            string output = "";

            instructions.ForEach(delegate (Instruction instruction)
            {
                output += instruction.ToString();
                output += '\n';
            });

            return output;
        }
    }
}

