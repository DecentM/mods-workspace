using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.Subtitles.Srt
{
    public class Parser
    {
        private Lexer lexer = new Lexer();
        private AstParser astParser = new AstParser();
        private Transformer transformer = new Transformer();

        public List<Instruction> Parse(string input)
        {
            List<Lexer.Token> tokens = this.lexer.Lex(input);
            AstParser.Ast ast = this.astParser.Parse(tokens);
            List<Instruction> instructions = this.transformer.ToInstructions(ast);

            return instructions;
        }
    }
}
