using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Vsi
{
    public class VsiCompiler : IntermediateCompiler
    {
        public override Ast CompileIntermediate(string input)
        {
            VsiLexer lexer = new VsiLexer();
            VsiParser parser = new VsiParser();

            List<VsiLexer.Token> tokens = lexer.Lex(input);

            return parser.Parse(tokens);
        }
    }
}
