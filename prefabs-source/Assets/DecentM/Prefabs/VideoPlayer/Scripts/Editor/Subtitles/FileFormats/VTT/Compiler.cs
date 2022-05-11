using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Vtt
{
    public class VttCompiler : IntermediateCompiler
    {
        public override IntermediateCompilationResult CompileIntermediate(string input)
        {
            VttLexer lexer = new VttLexer();
            VttParser parser = new VttParser();

            List<VttLexer.Token> tokens = lexer.Lex(input);
            Ast ast = parser.Parse(tokens);

            List<Node> errors = ast.GetUnknowns();

            return new IntermediateCompilationResult(errors, ast);
        }
    }
}
