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

        private int ParseTimestampFromIndex(List<SrtLexer.Token> tokens, int index)
        {
            int tCursor = index;
            SrtLexer.Token tCurrent = tokens.ElementAt(tCursor);
            string timestamp = "";

            // Keep going until we see a space or newline
            while (
                tCurrent.type != TokenType.Space
                && tCurrent.type != TokenType.Newline
            ) {
                tCursor++;
                tCurrent = tokens.ElementAt(tCursor);
                timestamp = $"{timestamp}{tCurrent.value}";
            }

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

        public override Ast Parse(List<SrtLexer.Token> tokens)
        {
            List<Node> nodes = new List<Node>();
            int cursor = 0;

            // Mode is just a NodeKind, to check where we currently are
            // The first thing in an .srt should be the screen index
            NodeKind mode = NodeKind.ScreenIndex;

            while (cursor < tokens.Count)
            {
                SrtLexer.Token current = tokens.ElementAt(cursor);

                // If we're expecting ScreenIndex
                if (mode == NodeKind.ScreenIndex)
                {
                    // Go until we see an int
                    // token type 2 == int
                    if (current.type != TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    int tCursor = cursor;
                    SrtLexer.Token tCurrent = tokens.ElementAt(tCursor);
                    string indexValue = "";

                    // Go until we see a newline
                    while (tCursor < tokens.Count && tCurrent.type != TokenType.Newline)
                    {
                        indexValue = $"{indexValue}{tCurrent.value}";

                        tCursor++;
                        tCurrent = tokens.ElementAtOrDefault(tCursor);

                        if (Object.Equals(tCurrent, default))
                        {
                            break;
                        }
                    }

                    if (indexValue == "")
                    {
                        Node unknownNode = new Node(NodeKind.Unknown, $"Cannot parse screen index in token {tCursor} because the parsed value is empty");
                        nodes.Add(unknownNode);
                    } else
                    {
                        Node node = new Node(NodeKind.ScreenIndex, indexValue);
                        nodes.Add(node);
                    }

                    cursor = tCursor;
                    // Move to expecting a start timestamp
                    mode = NodeKind.TimestampStart;
                    continue;
                }

                // If we're in the start timestamp
                if (mode == NodeKind.TimestampStart)
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
                    } catch (ArgumentException ex)
                    {
                        Node unknownNode = new Node(NodeKind.Unknown, ex.Message);
                        nodes.Add(unknownNode);
                        mode = NodeKind.TimestampArrow;
                        continue;
                    }

                    // node kind 2 == TimestampStart
                    Node node = new Node(NodeKind.TimestampStart, timestampMillis);
                    nodes.Add(node);

                    // Skip the timestamp + a space
                    cursor = cursor + 12;
                    // Move to expecting the arrow
                    mode = NodeKind.TimestampArrow;
                    continue;
                }

                // Expect the arrow
                if (mode == NodeKind.TimestampArrow)
                {
                    // Go until we see a hyphen
                    if (current.type != TokenType.Hyphen)
                    {
                        cursor++;
                        continue;
                    }

                    int tCursor = cursor;
                    SrtLexer.Token tCurrent = tokens.ElementAt(tCursor);
                    string body = "";

                    // Keep going until we see a space
                    while (tCurrent.type != TokenType.Space && tCurrent.type != TokenType.Unknown)
                    {
                        body = $"{body}{tCurrent.value}";
                        tCursor++;
                        tCurrent = tokens.ElementAtOrDefault(tCursor);

                        if (Object.Equals(tCurrent, default))
                        {
                            break;
                        }
                    }

                    // If we saw a valid arrow, add it as a Node, then move the cursor behind it
                    if (body == "-->")
                    {
                        // node kind 3 == TimestampArrow
                        Node node = new Node(NodeKind.TimestampArrow, body);
                        nodes.Add(node);

                        cursor = tCursor;
                    }
                    // If we didn't, advance the cursor by one and add an unknown node
                    else
                    {
                        Node unknownNode = new Node(NodeKind.Unknown, $"Cannot parse arrow: {current.value}");
                        nodes.Add(unknownNode);
                        cursor++;
                    }

                    // Move to expecting the second timestamp, even if we didn't see an arrow
                    // to prevent infinite loops.
                    mode = NodeKind.TimestampEnd;
                    continue;
                }

                // Expect the end timestamp
                if (mode == NodeKind.TimestampEnd)
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
                        mode = NodeKind.TimestampArrow;
                        continue;
                    }

                    // node kind 4 == TimestampEnd
                    Node node = new Node(NodeKind.TimestampEnd, timestampMillis);
                    nodes.Add(node);

                    // Skip the timestamp + a space
                    cursor = cursor + 12;
                    // Move to expecting the arrow
                    mode = NodeKind.TextContents;
                    continue;
                }

                // Expect the text contents
                if (mode == NodeKind.TextContents)
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
                    mode = NodeKind.ScreenIndex;
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
