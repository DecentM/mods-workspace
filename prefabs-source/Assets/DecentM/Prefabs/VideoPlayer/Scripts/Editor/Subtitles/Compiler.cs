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

        public static string[] SupportedFormats = { Srt, Vtt };
    }

    public static class SubtitleCompiler
    {
        public struct CompilationResultError
        {
            public string value;

            public CompilationResultError(string value = "")
            {
                this.value = value;
            }
        }

        public struct CompilationResult
        {
            public string output;
            public List<CompilationResultError> errors;

            public CompilationResult(List<CompilationResultError> errors, string output = "")
            {
                this.output = output;
                this.errors = errors;
            }
        }

        public static CompilationResult Compile(string source, string fileType)
        {
            CompilationResult result = new CompilationResult(new List<CompilationResultError>(), "");
            List<Instruction> instructions = new List<Instruction>();

            if (!FileTypes.IsSupported(fileType))
            {
                throw new NotImplementedException($"File type {fileType} is not supported. Use a different file, or convert your subtitles to a supported format: {String.Join(", ", FileTypes.SupportedFormats)}");
            }

            string sanitisedSource = new TextProcessing(source)
                .CRLFToLF()
                .ResolveHTMLEntities()
                .GetResult();

            switch (fileType)
            {
                case FileTypes.Srt:
                    {
                        Srt.Lexer lexer = new Srt.Lexer();
                        Srt.Parser parser = new Srt.Parser();
                        Srt.Transformer transformer = new Srt.Transformer();

                        List<Srt.Lexer.Token> tokens = lexer.Lex(sanitisedSource);
                        Srt.Parser.Ast ast = parser.Parse(tokens);
                        List<Srt.Parser.Node> errors = Srt.Parser.GetUnknowns(ast);

                        // Add all errors to the result struct so they can be displayed to the user
                        if (errors.Count != 0)
                        {
                            errors.ForEach(error =>
                            {
                                if (error.value != null)
                                {
                                    result.errors.Add(new CompilationResultError((error.value.ToString())));
                                }
                            });
                        }

                        instructions = transformer.ToInstructions(ast);
                        break;
                    }

                case FileTypes.Ass:
                case FileTypes.Usf:
                case FileTypes.Vtt:
                    {
                        Vtt.Lexer lexer = new Vtt.Lexer();
                        Vtt.Parser parser = new Vtt.Parser();
                        Vtt.Transformer transformer = new Vtt.Transformer();

                        List<Vtt.Lexer.Token> tokens = lexer.Lex(sanitisedSource);
                        Vtt.Parser.Ast ast = parser.Parse(tokens);
                        List<Vtt.Parser.Node> errors = Vtt.Parser.GetUnknowns(ast);

                        // Add all errors to the result struct so they can be displayed to the user
                        if (errors.Count != 0)
                        {
                            errors.ForEach(error =>
                            {
                                if (error.value != null)
                                {
                                    result.errors.Add(new CompilationResultError((error.value.ToString())));
                                }
                            });
                        }

                        instructions = transformer.ToInstructions(ast);
                        break;
                    }
                case FileTypes.Stl:
                case FileTypes.Sub:
                case FileTypes.Ssa:
                case FileTypes.Ttxt:
                default:
                    break;
            }

            result.output = "";

            instructions.ForEach((Instruction instruction) =>
            {
                result.output += instruction.ToString();
                result.output += '\n';
            });

            return result;
        }
    }
}

