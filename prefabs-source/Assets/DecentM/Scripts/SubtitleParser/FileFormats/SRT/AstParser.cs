using DecentM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Srt
{
    public class AstParser
    {
        public enum NodeKind
        {
            Unknown = 0,
            ScreenIndex = 1,
            TimestampStart = 2,
            TimestampArrow = 3,
            TimestampEnd = 4,
            TextContents = 5,
        }

        public struct Node
        {
            public Node(NodeKind kind, string value)
            {
                this.kind = kind;
                this.value = value;
            }

            public Node(NodeKind kind, int value)
            {
                this.kind = kind;
                this.value = value;
            }

            public readonly NodeKind kind;
            public readonly object value;
        }

        public struct Ast
        {
            public Ast(List<Node> nodes)
            {
                this.nodes = nodes;
                this.kind = "SubtitleParserAst";
            }

            public readonly string kind;
            public readonly List<Node> nodes;
        }

        private int ParseTimestampFromIndex(List<Lexer.Token> tokens, int index)
        {
            int tCursor = index;
            Lexer.Token tCurrent = tokens.ElementAt(tCursor);
            string timestamp = "";

            // Keep going until we see a space or newline
            while (tCurrent.type != Lexer.TokenType.Space && tCurrent.type != Lexer.TokenType.Newline)
            {
                tCursor++;
                tCurrent = tokens.ElementAt(tCursor);
                timestamp = $"{timestamp}{tCurrent.value}";
            }

            // timestamp format: 00:05:00,400
            // hours:minutes:seconds,milliseconds
            string[] parts = timestamp.Split(',');
            string time = parts[0];
            int millis = 0;
            int.TryParse(parts[1], out millis);

            string[] timeParts = time.Split(':');
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            int.TryParse(timeParts[0], out hours);
            int.TryParse(timeParts[1], out minutes);
            int.TryParse(timeParts[2], out seconds);

            // Add values to get the value in millis
            return millis + (seconds * 1000) + (minutes * 60 * 1000) + (hours * 60 * 60 * 1000);
        }

        public Ast Parse(List<Lexer.Token> tokens)
        {
            List<Node> nodes = new List<Node>();
            int cursor = 0;
            int mode = 1; // Node kind, to check where we currently are

            while (cursor < tokens.Count)
            {
                Lexer.Token current = tokens.ElementAt(cursor);

                // If we're expecting ScreenIndex
                if (mode == 1)
                {
                    // Go until we see an int
                    // token type 2 == int
                    if (current.type != Lexer.TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    int tCursor = cursor;
                    Lexer.Token tCurrent = tokens.ElementAt(tCursor);
                    string indexValue = "";

                    // Go until we see a newline
                    while (tCurrent.type != Lexer.TokenType.Newline)
                    {
                        indexValue = $"{indexValue}{tCurrent.value}";

                        tCursor++;
                        tCurrent = tokens[tCursor];
                    }

                    Node node = new Node(NodeKind.ScreenIndex, indexValue);
                    nodes.Add(node);

                    cursor = tCursor;
                    // Move to expecting a start timestamp
                    mode = 2;
                    continue;
                }

                // If we're in the start timestamp
                if (mode == 2)
                {
                    // Go until we see an int
                    // token type 2 == int
                    if (current.type != Lexer.TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    int timestampMillis = this.ParseTimestampFromIndex(tokens, cursor);

                    // node kind 2 == TimestampStart
                    Node node = new Node(NodeKind.TimestampStart, timestampMillis);
                    nodes.Add(node);

                    // Skip the timestamp + a space
                    cursor = cursor + 12;
                    // Move to expecting the arrow
                    mode = 3;
                    continue;
                }

                // Expect the arrow
                if (mode == 3)
                {
                    // Go until we see a hyphen
                    if (current.type != Lexer.TokenType.Hyphen)
                    {
                        cursor++;
                        continue;
                    }

                    int tCursor = cursor;
                    Lexer.Token tCurrent = tokens.ElementAt(tCursor);
                    string body = "";

                    // Keep going until we see a space
                    while (tCurrent.type != Lexer.TokenType.Space)
                    {
                        body = $"{body}{tCurrent.value}";
                        tCursor++;
                        tCurrent = tokens[tCursor];
                    }

                    if (body == "-->")
                    {
                        // node kind 3 == TimestampArrow
                        Node node = new Node(NodeKind.TimestampArrow, body);
                        nodes.Add(node);

                        cursor = tCursor;

                        // Move to expecting the second timestamp
                        mode = 4;
                    }
                    continue;
                }

                // Expect the end timestamp
                if (mode == 4)
                {
                    // Go until we see an int
                    // token type 2 == int
                    if (current.type != Lexer.TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    int timestampMillis = this.ParseTimestampFromIndex(tokens, cursor);

                    // node kind 4 == TimestampEnd
                    Node node = new Node(NodeKind.TimestampEnd, timestampMillis);
                    nodes.Add(node);

                    // Skip the timestamp + a space
                    cursor = cursor + 12;
                    // Move to expecting the arrow
                    mode = 5;
                    continue;
                }

                // Expect the text contents
                if (mode == 5)
                {
                    int tCursor = cursor;
                    int consecutiveNewlines = 0;
                    string textContents = "";

                    // Skip the first newline
                    if (current.type == Lexer.TokenType.Newline)
                    {
                        cursor++;
                        continue;
                    }
                    
                    // Two consecutive newlines mean the end of the current screen
                    while (consecutiveNewlines < 2)
                    {
                        if (tCursor >= tokens.Count)
                        {
                            // We've reached the end of the file
                            break;
                        }

                        Lexer.Token tCurrent = tokens.ElementAt(tCursor);

                        // If we've seen two consecutive newlines, it's the end of the text contents part,
                        // and we need to return to expecting the next subtitle screen
                        // tokenType 7 == newline
                        if (tCurrent.type == Lexer.TokenType.Newline)
                        {
                            consecutiveNewlines++;
                        }
                        else
                        {
                            consecutiveNewlines = 0;
                        }

                        textContents = $"{textContents}{tCurrent.value}";
                        tCursor++;
                    }

                    Node node = new Node(NodeKind.TextContents, textContents);
                    nodes.Add(node);

                    cursor = tCursor;
                    // Go back to expecting the next section's index
                    mode = 1;
                    continue;
                }
            }

            return new Ast(nodes);
        }
    }
}
