using System;
using System.Collections.Generic;
using DecentM.TextProcessing;

namespace DecentM.Subtitles
{
    public abstract class Lexer<TokenType>
    {
        public abstract List<Token> Lex(string text);

        public struct Token
        {
            public Token(TokenType type, string value)
            {
                this.type = type;
                this.value = value;
            }

            public Token(TokenType type, int value)
            {
                this.type = type;
                this.value = value;
            }

            public Token(TokenType type, object value)
            {
                this.type = type;
                this.value = value;
            }

            public Token(TokenType type)
            {
                this.type = type;
                this.value = null;
            }

            public readonly TokenType type;
            public readonly object value;
        }
    }

    public abstract class Parser<LexerType, TokenType> where LexerType : Lexer<TokenType>
    {
        protected int ParseTimestamp(string timestamp, char millisSeparator, char partsSeparator)
        {
            // timestamp format: 00:05:00,400
            // hours:minutes:seconds,milliseconds
            string[] parts = timestamp.Split(millisSeparator);

            int millis = 0;
            string time;

            switch (parts.Length)
            {
                case 1:
                    time = parts[0];
                    break;

                case 2:
                    time = parts[0];
                    int.TryParse(parts[1], out millis);
                    break;

                default:
                    return -1;
            }

            string[] timeParts = time.Split(partsSeparator);

            // If the timestamp is invalid, we return -1 to signal that parsing failed

            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            switch (timeParts.Length)
            {
                case 1:
                    int.TryParse(timeParts[0], out seconds);
                    break;

                case 2:
                    int.TryParse(timeParts[0], out minutes);
                    int.TryParse(timeParts[1], out seconds);
                    break;

                case 3:
                    int.TryParse(timeParts[0], out hours);
                    int.TryParse(timeParts[1], out minutes);
                    int.TryParse(timeParts[2], out seconds);
                    break;

                default:
                    return -1;
            }

            // Add values to get the value in millis
            return millis + (seconds * 1000) + (minutes * 60 * 1000) + (hours * 60 * 60 * 1000);
        }

        public abstract Ast Parse(List<Lexer<TokenType>.Token> tokens);
    }

    public abstract class Writer
    {
        protected Ast ast;

        public Writer(Ast ast)
        {
            this.ast = ast;
        }

        public override abstract string ToString();
    }

    public struct CompilationResultError
    {
        public string value;

        public CompilationResultError(string value = "")
        {
            this.value = value;
        }
    }

    public abstract class Compiler
    {
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

        public abstract CompilationResult Compile(string input, Transformer transformer);
    }

    public abstract class IntermediateCompiler
    {
        public abstract Ast CompileIntermediate(string input);
    }
}
