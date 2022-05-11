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
        public struct IntermediateCompilationResult
        {
            public Ast output;
            public List<Node> errors;

            public IntermediateCompilationResult(List<Node> errors, Ast output)
            {
                this.output = output;
                this.errors = errors;
            }
        }

        public abstract IntermediateCompilationResult CompileIntermediate(string input);
    }
}
