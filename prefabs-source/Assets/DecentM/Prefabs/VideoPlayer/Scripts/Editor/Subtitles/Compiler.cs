using System;
using System.Collections.Generic;
using System.Linq;

using DecentM.TextProcessing;

using DecentM.Subtitles.Srt;
using DecentM.Subtitles.Vtt;

namespace DecentM.Subtitles
{
    public static class SubtitleFormat
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
            return SubtitleFormat.SupportedFormats.Any(f => f == filetype);
        }

        public static string[] SupportedFormats = { Srt, Vtt };
    }

    public static class SubtitleCompiler
    {
        public static Compiler.CompilationResult Compile(string source, string fileType)
        {
            if (!SubtitleFormat.IsSupported(fileType))
            {
                throw new NotImplementedException($"File type {fileType} is not supported. Use a different file, or convert your subtitles to a supported format: {String.Join(", ", SubtitleFormat.SupportedFormats)}");
            }

            string sanitisedSource = new TextProcessor(source)
                .CRLFToLF()
                .ResolveHTMLEntities()
                .GetResult();

            Compiler compiler = null;

            switch (fileType)
            {
                case SubtitleFormat.Srt:
                    compiler = new SrtCompiler();
                    break;

                case SubtitleFormat.Ass:
                case SubtitleFormat.Usf:
                case SubtitleFormat.Vtt:
                    compiler = new VttCompiler();
                    break;
                case SubtitleFormat.Stl:
                case SubtitleFormat.Sub:
                case SubtitleFormat.Ssa:
                case SubtitleFormat.Ttxt:
                default:
                    break;
            }

            return compiler.Compile(sanitisedSource);
        }
    }
}
