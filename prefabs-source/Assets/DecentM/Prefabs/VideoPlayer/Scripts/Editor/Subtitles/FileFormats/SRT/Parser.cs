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

        private int ParseTimestamp(string timestamp)
        {
            // timestamp format: 00:05:00,400
            // hours:minutes:seconds,milliseconds
            string[] parts = timestamp.Split(',');

            // If the timestamp is invalid, we return zero to make the instruction runner
            // ignore this screen.
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Cannot parse timestamp {timestamp}, because it cannot be split into two parts on a comma");
            }

            string time = parts[0];
            int millis = 0;
            int.TryParse(parts[1], out millis);

            string[] timeParts = time.Split(':');

            // If the timestamp is invalid, we return zero to make the instruction runner
            // ignore this screen.
            if (timeParts.Length != 3)
            {
                throw new ArgumentException($"Cannot parse timestamp {timestamp}, because it cannot be split into three parts on a colon");
            }

            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            int.TryParse(timeParts[0], out hours);
            int.TryParse(timeParts[1], out minutes);
            int.TryParse(timeParts[2], out seconds);

            // Add values to get the value in millis
            return millis + (seconds * 1000) + (minutes * 60 * 1000) + (hours * 60 * 60 * 1000);
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

            string ConsumeUntil(TokenType type)
            {
                int tCursor = cursor;
                SrtLexer.Token tCurrent = tokens.ElementAt(tCursor);
                string result = "";

                while (tCurrent.type != type && tCursor < tokens.Count)
                {
                    result += (string)tCurrent.value;
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
                        SrtLexer.Token next = tokens.ElementAt(cursor);

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

                    string timestamp = ConsumeTimestamp();

                    try
                    {
                        timestampMillis = this.ParseTimestampFromIndex(tokens, cursor);
                    } catch (ArgumentException ex)
                    {
                        Node unknownNode = new Node(NodeKind.Unknown, ex.Message);
                        nodes.Add(unknownNode);
                        mode = Mode.ExpectingArrow;
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

                    int timestampMillis = -1;

                    try
                    {
                        timestampMillis = this.ParseTimestampFromIndex(tokens, cursor);
                    }
                    catch (ArgumentException ex)
                    {
                        Node unknownNode = new Node(NodeKind.Unknown, ex.Message);
                        nodes.Add(unknownNode);
                        mode = Mode.ExpectingArrow;
                        continue;
                    }

                    // node kind 4 == TimestampEnd
                    Node node = new Node(NodeKind.TimestampEnd, timestampMillis);
                    nodes.Add(node);

                    // Skip the timestamp + a space
                    cursor = cursor + 12;
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
                        Node unknownNode = new Node(NodeKind.Unknown, $"Cannot parse text contents in token {tCursor} because the parsed value is empty");
                        nodes.Add(unknownNode);
                    } else
                    {
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
