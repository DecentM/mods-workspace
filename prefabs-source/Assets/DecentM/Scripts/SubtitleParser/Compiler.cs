using System;
using System.Collections.Generic;
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
            return FileTypes.SupportedFormats.Any(f => f == filetype);
        }

        public static string[] SupportedFormats = { Srt };
    }

    public class Compiler
    {
        private Srt.Lexer srtLexer = new Srt.Lexer();
        private Srt.Parser srtParser = new Srt.Parser();
        private Srt.Transformer srtTransformer = new Srt.Transformer();

        public struct CompilationResultError
        {
            public string value = "";

            public CompilationResultError(string value = "")
            {
                this.value = value;
            }
        }

        public struct CompilationResult
        {
            public string output = "";
            public List<CompilationResultError> errors = new List<CompilationResultError>();
        }

        public CompilationResult Compile(string source, string fileType)
        {
            CompilationResult result = new CompilationResult();
            List<Instruction> instructions = new List<Instruction>();

            if (!FileTypes.IsSupported(fileType))
            {
                throw new NotImplementedException($"File type {fileType} is not supported. Use a different file, or convert your subtitles to a supported format: {String.Join(", ", FileTypes.SupportedFormats)}");
            }

            switch (fileType)
            {
                case FileTypes.Srt:
                    List<Srt.Lexer.Token> tokens = this.srtLexer.Lex(source);
                    Srt.Parser.Ast ast = this.srtParser.Parse(tokens);
                    List<Srt.Parser.Node> errors = Srt.Parser.GetUnknowns(ast);

                    // Add all errors to the result struct so they can be displayed to the user
                    if (errors.Count != 0)
                    {
                        errors.ForEach(error =>
                        {
                            if (error.value != null)
                            {
                                result.errors.Add(new CompilationResultError(((string)error.value)));
                            }
                        });
                    }

                    instructions = this.srtTransformer.ToInstructions(ast);
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

            result.output = "";

            instructions.ForEach(delegate (Instruction instruction)
            {
                result.output += instruction.ToString();
                result.output += '\n';
            });

            return result;
        }
    }
}

