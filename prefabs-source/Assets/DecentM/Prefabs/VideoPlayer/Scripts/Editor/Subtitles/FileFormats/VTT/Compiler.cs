using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecentM.Subtitles.Vtt
{
    public class VttCompiler : IntermediateCompiler<NodeKind>
    {
        public override IntermediateCompilationResult CompileIntermediate(string input)
        {
            VttLexer lexer = new VttLexer();
            VttParser parser = new VttParser();

            List<VttLexer.Token> tokens = lexer.Lex(input);
            Ast<NodeKind> ast = parser.Parse(tokens);

            ast = new VttTransformer(ast)
                .LigaturiseArabicText()
                .Transform();

            List<Node<NodeKind>> errors = VttParser.GetUnknowns(ast);

            return new IntermediateCompilationResult(errors, ast);
        }

        public override CompilationResult Compile(string input)
        {
            VttWriter writer = new VttWriter();

            IntermediateCompilationResult intermediateResult = this.CompileIntermediate(input);
            List<CompilationResultError> errors = new List<CompilationResultError>();

            foreach (Node<NodeKind> unknownNode in intermediateResult.errors)
            {
                errors.Add(new CompilationResultError(unknownNode.value.ToString()));
            }

            return new CompilationResult(errors, string.Join("\n", writer.ToInstructions(intermediateResult.output).Select(instruction => instruction.ToString()).ToArray()));
        }
    }
}
