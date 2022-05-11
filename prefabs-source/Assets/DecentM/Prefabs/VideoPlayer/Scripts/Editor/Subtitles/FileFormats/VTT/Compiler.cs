using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Vtt
{
    public class VttCompiler : IntermediateCompiler
    {
        public override Ast CompileIntermediate(string input)
        {
            VttLexer lexer = new VttLexer();
            VttParser parser = new VttParser();

            List<VttLexer.Token> tokens = lexer.Lex(input);
            return parser.Parse(tokens);
        }
    }
}
