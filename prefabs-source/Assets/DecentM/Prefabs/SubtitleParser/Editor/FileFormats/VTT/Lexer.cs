using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DecentM.Subtitles.Vtt
{
    public class Lexer
    {
        public enum TokenType
        {
            Unknown,
            Char,
            Number,
            Hyphen,
            SidewaysCaret,
            Colon,
            Comma,
            Newline,
            Space,
            WEBVTTHeader,
            Note,
            DoubleNewline,
            Style,
        }

        public struct Token
        {
            public Token(TokenType type, string value)
            {
                this.type = type;
                this.value = value;
            }

            public Token(TokenType type, int value)
            {
                this.type = type;
                this.value = value;
            }

            public Token(TokenType type, object value)
            {
                this.type = type;
                this.value = value;
            }

            public readonly TokenType type;
            public readonly object value;
        }

        public List<Token> Lex(string text)
        {
            int cursor = 0;
            List<Token> tokens = new List<Token>();

            void AddToken(TokenType type, object value)
            {
                Token token = new Token(type, value);
                tokens.Add(token);
                cursor++;
            }

            string ConsumeUntilDoubleNewline(int skip = 0)
            {
                string result = "";
                cursor += skip;

                bool lastCharWasNewline = false;
                while (cursor < text.Length)
                {
                    result += text[cursor];
                    cursor++;

                    if (text[cursor] == '\n' && lastCharWasNewline) break;
                    if (text[cursor] == '\n') lastCharWasNewline = true;
                }

                return result;
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

            bool CheckNextCharIsNewline()
            {
                if (cursor >= text.Length - 1) return false;

                bool nextIsCR = text[cursor + 1] == '\r';
                return text[cursor + (nextIsCR ? 2 : 1)] == '\n';
            }

            bool CheckPreviousCharIsNewline()
            {
                if (cursor == 0) return false;

                bool nextIsCR = text[cursor - 1] == '\r';
                return text[cursor - (nextIsCR ? 2 : 1)] == '\n';
            }

            while (cursor < text.Length)
            {
                char current = text[cursor];

                string noteWord = "NOTE";
                if (FindWord(noteWord))
                {
                    string noteValue = ConsumeUntilDoubleNewline(noteWord.Length);
                    AddToken(TokenType.Note, noteValue);
                    AddToken(TokenType.DoubleNewline, "\n\n");
                    cursor--;
                    continue;
                }

                string webvttWord = "WEBVTT";
                if (FindWord(webvttWord))
                {
                    string value = ConsumeUntilDoubleNewline(webvttWord.Length);
                    AddToken(TokenType.WEBVTTHeader, value);
                    AddToken(TokenType.DoubleNewline, "\n\n");
                    cursor--;
                    continue;
                }

                string styleWord = "STYLE";
                if (FindWord(styleWord))
                {
                    string value = ConsumeUntilDoubleNewline(styleWord.Length);
                    AddToken(TokenType.Style, value);
                    AddToken(TokenType.DoubleNewline, "\n\n");
                    cursor--;
                    continue;
                }

                // Hyphens are in arrows, and text
                if (current == '-')
                {
                    AddToken(TokenType.Hyphen, "-");
                    continue;
                }

                // Sideways carets are in arrows, and text
                if (current == '>')
                {
                    AddToken(TokenType.SidewaysCaret, ">");
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
                    if (CheckNextCharIsNewline())
                    {
                        AddToken(TokenType.DoubleNewline, "\n\n");
                        continue;
                    }
                    
                    if (!CheckPreviousCharIsNewline())
                    {
                        AddToken(TokenType.Newline, "\n");
                        continue;
                    }

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
                    AddToken(TokenType.Space, " ");
                    continue;
                }

                int intValue;
                if (int.TryParse(current.ToString(), out intValue) == true)
                {
                    AddToken(TokenType.Number, intValue);
                    continue;
                }

                // If we're down here, that means we have a char, so we just treat it as part of
                // the subtitle text
                AddToken(TokenType.Char, current.ToString());
                continue;
            }

            return tokens;
        }
    }
}
