using System;
using System.Linq;
using System.Collections.Generic;

using DecentM.EditorTools;

using DecentM.Prefabs.Subtitles.Vsi;
using DecentM.Prefabs.Subtitles.Srt;
using DecentM.Prefabs.Subtitles.Vtt;

namespace DecentM.Prefabs.Subtitles
{
    public static class SubtitleFormat
    {
        // .vsi is our own custom format
        public const string Vsi = ".vsi";

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

        public static string[] SupportedFormats = { Srt, Vtt, Vsi };
    }

    public static class SubtitleCompiler
    {
        public static Ast CompileIntermediate(string source, string inFileType)
        {
            if (!SubtitleFormat.IsSupported(inFileType))
            {
                throw new NotImplementedException(
                    $"File type {inFileType} is not supported. Use a different file, or convert your subtitles to a supported format: {String.Join(", ", SubtitleFormat.SupportedFormats)}"
                );
            }

            string sanitisedSource = new TextProcessor(source)
                .CRLFToLF()
                .ResolveHTMLEntities()
                .GetResult();

            Parser parser = null;

            switch (inFileType)
            {
                case SubtitleFormat.Srt:
                    parser = new SrtParser();
                    break;

                case SubtitleFormat.Vtt:
                    parser = new VttParser();
                    break;

                case SubtitleFormat.Vsi:
                    parser = new VsiParser();
                    break;

                default:
                    break;
            }

            if (parser == null)
                throw new NotImplementedException(
                    $"No parser exists for this format: {inFileType}"
                );

            return parser.Parse(sanitisedSource);
        }

        public static string Write(Ast ast, string outFileType)
        {
            Writer writer = null;

            switch (outFileType)
            {
                case SubtitleFormat.Srt:
                    writer = new SrtWriter(ast);
                    break;

                case SubtitleFormat.Vtt:
                    writer = new VttWriter(ast);
                    break;

                case SubtitleFormat.Vsi:
                    writer = new VsiWriter(ast);
                    break;

                default:
                    break;
            }

            if (writer == null)
                throw new NotImplementedException(
                    $"No writer exists for this format: {outFileType}"
                );

            return writer.ToString();
        }

        public static Compiler.CompilationResult Compile(
            string source,
            string inFileType,
            string outFileType
        )
        {
            Ast result = CompileIntermediate(source, inFileType);

            Transformer transformer = new Transformer().LigaturiseArabicText();

            List<CompilationResultError> errors = new List<CompilationResultError>();
            errors.AddRange(
                result.nodes
                    .Where(node => node.kind == NodeKind.Unknown)
                    .Select(err => new CompilationResultError(err.value.ToString()))
            );

            Ast newAst = transformer.Transform(result);
            string output = Write(newAst, outFileType);

            return new Compiler.CompilationResult(errors, output);
        }
    }
}
