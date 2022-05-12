using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Srt
{
    public enum TokenType
    {
        Unknown,
        Char,
        Number,
        Arrow,
        Colon,
        Comma,
        Newline,
        Space,
    }

    public class SrtLexer : Lexer<TokenType>
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

            bool FindWord(string word)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    if (cursor + i >= text.Length) return false;
                    if (word[i] != text[cursor + i]) return false;
                }

                return true;
            }

            while (cursor < text.Length)
            {
                char current = text[cursor];

                if (current == '-' && FindWord("-->"))
                {
                    AddToken(TokenType.Arrow, "-->");
                    cursor += 2;
                    continue;
                }

                if (current == ':')
                {
                    AddToken(TokenType.Colon, ":");
                    continue;
                }

                if (current == ',')
                {
                    AddToken(TokenType.Comma, ",");
                    continue;
                }

                if (current == '\n')
                {
                    AddToken(TokenType.Newline, "\n");
                    continue;
                }

                if (current == ' ')
                {
                    AddToken(TokenType.Space, " ");
                    continue;
                }

                int intValue;
                if (int.TryParse(current.ToString(), out intValue) == true)
                {
                    Token intToken = new Token(TokenType.Number, intValue);
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
