using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DecentM.Prefabs.Subtitles.Vsi
{
    public enum TokenType
    {
        InstructionLine,
        Newline,
    }

    public class VsiLexer : Lexer<TokenType>
    {
        public override List<Token> Lex(string text)
        {
            int cursor = 0;
            List<Token> tokens = new List<Token>();

            void AddToken(TokenType type, object value)
            {
                Token token = new Token(type, value);
                tokens.Add(token);
                cursor++;
            }

            string ConsumeUntilNewline(int skip = 0)
            {
                string result = "";
                cursor += skip;

                while (cursor < text.Length - 1)
                {
                    if (text[cursor] == '\n')
                        break;

                    result += text[cursor];
                    cursor++;
                }

                return result;
            }

            while (cursor < text.Length)
            {
                char current = text[cursor];

                if (current == '\n')
                {
                    AddToken(TokenType.Newline, "\n");
                    continue;
                }

                string lineValue = ConsumeUntilNewline();
                AddToken(TokenType.InstructionLine, lineValue);
                continue;
            }

            return tokens;
        }
    }
}
