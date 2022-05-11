using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Srt
{
    public class SrtCompiler : IntermediateCompiler
    {
        public override Ast CompileIntermediate(string input)
        {
            SrtLexer lexer = new SrtLexer();
            SrtParser parser = new SrtParser();

            List<SrtLexer.Token> tokens = lexer.Lex(input);
            return parser.Parse(tokens);
        }
    }
}
