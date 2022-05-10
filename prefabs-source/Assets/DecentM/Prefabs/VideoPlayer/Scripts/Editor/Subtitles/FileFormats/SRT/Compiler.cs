using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecentM.Subtitles.Srt
{
    public class SrtCompiler : IntermediateCompiler<NodeKind>
    {
        private Transformer<NodeKind> transformer;

        public override IntermediateCompilationResult CompileIntermediate(string input)
        {
            SrtLexer lexer = new SrtLexer();
            SrtParser parser = new SrtParser();

            List<SrtLexer.Token> tokens = lexer.Lex(input);
            Ast<NodeKind> ast = parser.Parse(tokens);

            ast = new SrtTransformer(ast)
                .LigaturiseArabicText()
                .Transform();

            List<Node<NodeKind>> errors = SrtParser.GetUnknowns(ast);

            return new IntermediateCompilationResult(errors, ast);
        }

        public override CompilationResult Compile(string input)
        {
            SrtWriter writer = new SrtWriter();
            List<CompilationResultError> errors = new List<CompilationResultError>();
            IntermediateCompilationResult intermediateResult = this.CompileIntermediate(input);

            foreach (Node<NodeKind> unknownNode in intermediateResult.errors)
            {
                errors.Add(new CompilationResultError(unknownNode.value.ToString()));
            }

            return new CompilationResult(errors, string.Join("\n", writer.ToInstructions(intermediateResult.output).Select(instruction => instruction.ToString()).ToArray()));
        }
    }
}
