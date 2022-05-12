using System;
using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Srt
{
    public class SrtParser : Parser<SrtLexer, TokenType>
    {
        public static List<Node> GetUnknowns(Ast ast)
        {
            return ast.nodes.Where(node => node.kind == NodeKind.Unknown).ToList();
        }

        private enum Mode
        {
            ExpectingScreenIndex,
            ExpectingStartTimestamp,
            ExpectingArrow,
            ExpectingEndTimestamp,
            ExpectingTextContent,
        }

        public override Ast Parse(List<SrtLexer.Token> tokens)
        {
            List<Node> nodes = new List<Node>();
            int cursor = 0;

            // Mode is just a NodeKind, to check where we currently are
            // The first thing in an .srt should be the screen index
            Mode mode = Mode.ExpectingScreenIndex;

            string ConsumeUntil(params TokenType[] type)
            {
                int tCursor = cursor;
                SrtLexer.Token tCurrent = tokens.ElementAt(tCursor);
                string result = "";

                while (!type.Contains(tCurrent.type) && tCursor < tokens.Count)
                {
                    result += tCurrent.value.ToString();
                    tCursor++;
                    tCurrent = tokens.ElementAt(tCursor);
                }

                cursor = tCursor;
                return result;
            }

            while (cursor < tokens.Count)
            {
                SrtLexer.Token current = tokens.ElementAt(cursor);

                // If we're expecting ScreenIndex
                if (mode == Mode.ExpectingScreenIndex)
                {
                    // Go until we see an int
                    // token type 2 == int
                    if (current.type != TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    if (cursor < tokens.Count - 1)
                    {
                        SrtLexer.Token next = tokens.ElementAt(cursor + 1);

                        if (next.type != TokenType.Newline)
                        {
                            cursor++;
                            continue;
                        }
                    }

                    // Move to expecting a start timestamp
                    mode = Mode.ExpectingStartTimestamp;
                    continue;
                }

                // If we're in the start timestamp
                if (mode == Mode.ExpectingStartTimestamp)
                {
                    // Go until we see an int
                    // token type 2 == int
                    if (current.type != TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    string timestamp = ConsumeUntil(TokenType.Space);
                    int timestampMillis = this.ParseTimestamp(timestamp, ',', ':');

                    if (timestampMillis == -1)
                    {
                        Node errorNode = new Node(
                            NodeKind.Unknown,
                            $"Failed to parse start timestamp: {timestamp}"
                        );
                        nodes.Add(errorNode);

                        // Don't change the mode, if we're expecting a start timestamp, we should go until we find one.
                        cursor++;
                        continue;
                    }

                    // node kind 2 == TimestampStart
                    Node node = new Node(NodeKind.TimestampStart, timestampMillis);
                    nodes.Add(node);

                    // Move to expecting the arrow
                    mode = Mode.ExpectingArrow;
                    continue;
                }

                // Expect the arrow
                if (mode == Mode.ExpectingArrow)
                {
                    // Go until we see an arrow
                    if (current.type != TokenType.Arrow)
                    {
                        cursor++;
                        continue;
                    }

                    // Move to expecting the second timestamp, even if we didn't see an arrow
                    // to prevent infinite loops.
                    mode = Mode.ExpectingEndTimestamp;
                    continue;
                }

                // Expect the end timestamp
                if (mode == Mode.ExpectingEndTimestamp)
                {
                    // Go until we see an int
                    // token type 2 == int
                    if (current.type != TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    string timestamp = ConsumeUntil(TokenType.Space, TokenType.Newline);
                    int timestampMillis = this.ParseTimestamp(timestamp, ',', ':');

                    if (timestampMillis == -1)
                    {
                        Node errorNode = new Node(
                            NodeKind.Unknown,
                            $"Failed to parse end timestamp: {timestamp}"
                        );
                        nodes.Add(errorNode);

                        // Don't change the mode, if we're expecting a start timestamp, we should go until we find one.
                        cursor++;
                        continue;
                    }

                    // node kind 4 == TimestampEnd
                    Node node = new Node(NodeKind.TimestampEnd, timestampMillis);
                    nodes.Add(node);

                    // Move to expecting the arrow
                    mode = Mode.ExpectingTextContent;
                    continue;
                }

                // Expect the text contents
                if (mode == Mode.ExpectingTextContent)
                {
                    int tCursor = cursor;
                    int consecutiveNewlines = 0;
                    string textContents = "";

                    // Skip the first newline
                    if (current.type == TokenType.Newline)
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

                        SrtLexer.Token tCurrent = tokens.ElementAt(tCursor);

                        // If we've seen two consecutive newlines, it's the end of the text contents part,
                        // and we need to return to expecting the next subtitle screen
                        // tokenType 7 == newline
                        if (tCurrent.type == TokenType.Newline)
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

                    if (textContents == "")
                    {
                        Node unknownNode = new Node(
                            NodeKind.Unknown,
                            $"Cannot parse text contents in token {tCursor} because the parsed value is empty"
                        );
                        nodes.Add(unknownNode);
                    }
                    else
                    {
                        while (textContents.EndsWith("\n"))
                        {
                            textContents = textContents.Remove(textContents.Length - 1, 1);
                        }

                        while (textContents.StartsWith("\n"))
                        {
                            textContents = textContents.Remove(0, 1);
                        }

                        Node node = new Node(NodeKind.TextContents, textContents);
                        nodes.Add(node);
                    }

                    cursor = tCursor;
                    // Go back to expecting the next section's index
                    mode = Mode.ExpectingScreenIndex;
                    continue;
                }

                // If none of the modes did anything, we move the cursor forward by one to try to salvage the source file.
                // Screens or other data might be missing but at least most of the file will be parsed still
                cursor++;
            }

            return new Ast(NodeKind.Root, nodes);
        }
    }
}
