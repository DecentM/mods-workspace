using System;
using System.Linq;
using System.Collections.Generic;

using DecentM.TextProcessing;

using DecentM.Subtitles.Srt;
using DecentM.Subtitles.Vtt;

namespace DecentM.Subtitles
{
    public static class SubtitleFormat
    {
        public const string VrcSI = ".vrcsi";

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
            return SupportedFormats.Any(f => f == filetype);
        }

        public static string[] SupportedFormats = { Srt, Vtt };
    }

    public static class SubtitleCompiler
    {
        public static Compiler.CompilationResult Compile(string source, string fileType, string outFileType)
        {
            if (!SubtitleFormat.IsSupported(fileType))
            {
                throw new NotImplementedException($"File type {fileType} is not supported. Use a different file, or convert your subtitles to a supported format: {String.Join(", ", SubtitleFormat.SupportedFormats)}");
            }

            string sanitisedSource = new TextProcessor(source)
                .CRLFToLF()
                .ResolveHTMLEntities()
                .GetResult();

            IntermediateCompiler compiler = null;

            switch (fileType)
            {
                case SubtitleFormat.Srt:
                    compiler = new SrtCompiler();
                    break;

                case SubtitleFormat.Vtt:
                    compiler = new VttCompiler();
                    break;

                default:
                    break;
            }

            if (compiler == null) throw new NotImplementedException($"No compiler exists for this format: {fileType}");

            Transformer transformer = new Transformer()
                .LigaturiseArabicText();

            List<CompilationResultError> errors = new List<CompilationResultError>();

            IntermediateCompiler.IntermediateCompilationResult result = compiler.CompileIntermediate(sanitisedSource);
            errors.AddRange(result.errors.Select(err => new CompilationResultError(err.value.ToString())));

            Ast newAst = transformer.Transform(result.output);
            Writer writer = null;

            switch (outFileType)
            {
                case SubtitleFormat.Srt:
                    writer = new SrtWriter(newAst);
                    break;

                case SubtitleFormat.Vtt:
                    writer = new VttWriter(newAst);
                    break;

                default:
                    break;
            }

            if (writer == null) throw new NotImplementedException($"No writer exists for this format: {outFileType}");

            string output = writer.ToString();

            return new Compiler.CompilationResult(errors, output);
        }
    }
}
