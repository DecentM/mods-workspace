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
            //object[][] tokens = new object[0][];
            List<Token> tokens = new List<Token>();

            while (cursor < text.Length)
            {
                char current = text[cursor];

                // Hyphens are in arrows, and text
                if (current == '-')
                {
                    Token token = new Token(TokenType.Hyphen, "-");

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                // Sideways carets are in arrows, and text
                if (current == '>')
                {
                    Token token = new Token(TokenType.SidewaysCaret, ">");

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                // int.Parse throws and error if passed a string that we can't catch because Udon, so we have a bit of a nightmare here
                if (current == '0')
                {
                    Token token = new Token(TokenType.Number, 0);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '1')
                {
                    Token token = new Token(TokenType.Number, 1);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '2')
                {
                    Token token = new Token(TokenType.Number, 2);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '3')
                {
                    Token token = new Token(TokenType.Number, 3);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '4')
                {
                    Token token = new Token(TokenType.Number, 4);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '5')
                {
                    Token token = new Token(TokenType.Number, 5);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '6')
                {
                    Token token = new Token(TokenType.Number, 6);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '7')
                {
                    Token token = new Token(TokenType.Number, 7);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '8')
                {
                    Token token = new Token(TokenType.Number, 8);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '9')
                {
                    Token token = new Token(TokenType.Number, 9);

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == ':')
                {
                    Token token = new Token(TokenType.Colon, ":");

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == ',')
                {
                    Token token = new Token(TokenType.Comma, ",");

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '\n')
                {
                    Token token = new Token(TokenType.Newline, "\n");

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                if (current == '\r')
                {
                    // Autoconvert CRLF to LF by ignoring \r
                    cursor++;
                    continue;
                }

                if (current == ' ')
                {
                    Token token = new Token(TokenType.Space, " ");

                    tokens.Add(token);

                    cursor++;
                    continue;
                }

                // If we're down here, that means we have a char, so we just treat it as part of
                // the subtitle text
                Token charToken = new Token(TokenType.Char, current.ToString());

                tokens.Add(charToken);

                cursor++;
                continue;
            }

            return tokens;
        }
    }
}
