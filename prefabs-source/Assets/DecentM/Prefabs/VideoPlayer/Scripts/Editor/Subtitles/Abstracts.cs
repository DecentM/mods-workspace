using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            public readonly TokenType type;
            public readonly object value;
        }
    }

    public struct Node<NodeKind>
    {
        public Node(NodeKind kind, string value)
        {
            this.kind = kind;
            this.value = value;
        }

        public Node(NodeKind kind, int value)
        {
            this.kind = kind;
            this.value = value;
        }

        public Node(NodeKind kind, Dictionary<string, string> value)
        {
            this.kind = kind;
            this.value = value;
        }

        public readonly NodeKind kind;
        public readonly object value;
    }

    public struct Ast<NodeKind>
    {
        public Ast(NodeKind kind, List<Node<NodeKind>> nodes)
        {
            this.nodes = nodes;
            this.kind = kind;
        }

        public Ast(NodeKind kind)
        {
            this.nodes = new List<Node<NodeKind>>();
            this.kind = kind;
        }

        public readonly NodeKind kind;
        public readonly List<Node<NodeKind>> nodes;

        public string Dump()
        {
            string result = "";

            foreach (Node<NodeKind> node in this.nodes)
            {
                result += $"{node.kind.ToString()}\n";
                result += $"{node.value.ToString()}\n";
                result += $"==================\n";
            }

            return result;
        }
    }

    public abstract class Parser<NodeKind, LexerType, TokenType> where LexerType : Lexer<TokenType>
    {
        public abstract Ast<NodeKind> Parse(List<Lexer<TokenType>.Token> tokens);
    }

    public abstract class Transformer<NodeKind>
    {
        protected Ast<NodeKind> input;

        public abstract Transformer<NodeKind> LigaturiseArabicText();

        public Ast<NodeKind> Transform()
        {
            return this.input;
        }
    }

    public abstract class Writer<ParserType, NodeKind, LexerType, TokenType> where LexerType : Lexer<TokenType> where ParserType : Parser<NodeKind, LexerType, TokenType>
    {
        public abstract string ToString(Ast<NodeKind> ast);
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

        public abstract CompilationResult Compile(string input);
    }

    public abstract class IntermediateCompiler<NodeKind> : Compiler
    {
        public struct IntermediateCompilationResult
        {
            public Ast<NodeKind> output;
            public List<Node<NodeKind>> errors;

            public IntermediateCompilationResult(List<Node<NodeKind>> errors, Ast<NodeKind> output)
            {
                this.output = output;
                this.errors = errors;
            }
        }

        public abstract IntermediateCompilationResult CompileIntermediate(string input);
    }
}
