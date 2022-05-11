using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Srt
{
    public enum TokenType
    {
        Unknown = 0,
        Char = 1,
        Number = 2,
        Hyphen = 3,
        SidewaysCaret = 4,
        Colon = 5,
        Comma = 6,
        Newline = 7,
        Space = 8,
    }

    public class SrtLexer : Lexer<TokenType>
    {
        public override List<Token> Lex(string text)
        {
            int cursor = 0;
            List<Token> tokens = new List<Token>();

            void AddToken(TokenType type)
            {
                Token token = new Token(type);
                tokens.Add(token);
                cursor++;
            }

            while (cursor < text.Length)
            {
                char current = text[cursor];

                // Hyphens are in arrows, and text
                if (current == '-')
                {
                    AddToken(TokenType.Hyphen);
                    continue;
                }

                // Sideways carets are in arrows, and text
                if (current == '>')
                {
                    AddToken(TokenType.SidewaysCaret);
                    continue;
                }

                if (current == ':')
                {
                    AddToken(TokenType.Colon);
                    continue;
                }

                if (current == ',')
                {
                    AddToken(TokenType.Comma);
                    continue;
                }

                if (current == '\n')
                {
                    AddToken(TokenType.Newline);
                    continue;
                }

                if (current == ' ')
                {
                    AddToken(TokenType.Space);
                    continue;
                }

                int intValue;
                if (int.TryParse(current.ToString(), out intValue) == true)
                {
                    Token intToken = new Token(TokenType.Char, intValue);
                    tokens.Add(intToken);
                    cursor++;
                    continue;
                }

                // If we're down here, that means we have a char, so we just treat it as part of
                // the subtitle text
                Token token = new Token(TokenType.Char, current.ToString());
                tokens.Add(token);
                cursor++;
                continue;
            }

            return tokens;
        }
    }
}
