using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Srt
{
    public class SrtCompiler : IntermediateCompiler
    {
        public override IntermediateCompilationResult CompileIntermediate(string input)
        {
            SrtLexer lexer = new SrtLexer();
            SrtParser parser = new SrtParser();

            List<SrtLexer.Token> tokens = lexer.Lex(input);
            Ast ast = parser.Parse(tokens);

            List<Node> errors = ast.GetUnknowns();

            return new IntermediateCompilationResult(errors, ast);
        }
    }
}
