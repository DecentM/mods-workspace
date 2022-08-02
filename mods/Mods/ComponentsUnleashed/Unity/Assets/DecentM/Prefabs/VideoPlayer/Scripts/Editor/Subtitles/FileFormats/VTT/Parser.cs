using System;
using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Vtt
{
    public class VttParser : Parser<VttLexer, TokenType>
    {
        private enum Mode
        {
            ExpectingHeader,
            ExpectingStartTimestamp,
            ExpectingArrow,
            ExpectingEndTimestamp,
            ExpectingTextContent,
            ExpectingTextParameters,
        }

        public override Ast Parse(string input)
        {
            VttLexer lexer = new VttLexer();

            List<VttLexer.Token> tokens = lexer.Lex(input);
            return this.Parse(tokens);
        }

        public override Ast Parse(List<VttLexer.Token> tokens)
        {
            List<Node> nodes = new List<Node>();
            int cursor = 0;

            string ConsumeUntil(params TokenType[] type)
            {
                int tCursor = cursor;
                VttLexer.Token tCurrent = tokens.ElementAt(tCursor);
                string result = "";

                while (!type.Contains(tCurrent.type) && tCursor < tokens.Count)
                {
                    result += tCurrent.value.ToString();
                    tCursor++;
                    tCurrent = tokens.ElementAt(tCursor);
                }

                tCursor--;
                cursor = tCursor;
                return result;
            }

            // The first thing in a .vtt should be the VTT header.
            Mode mode = Mode.ExpectingHeader;

            while (cursor < tokens.Count)
            {
                VttLexer.Token current = tokens.ElementAt(cursor);

                // Ignore notes as they're just comments
                if (current.type == TokenType.Note)
                {
                    cursor++;
                    continue;
                }

                // Ignore styles (for now) as they're probably hard to convert into
                // TMPro styles.
                if (current.type == TokenType.Style)
                {
                    cursor++;
                    continue;
                }

                if (mode == Mode.ExpectingHeader)
                {
                    if (current.type != TokenType.WEBVTTHeader)
                    {
                        cursor++;
                        continue;
                    }

                    mode = Mode.ExpectingStartTimestamp;
                    cursor++;
                    continue;
                }

                if (mode == Mode.ExpectingStartTimestamp)
                {
                    // Go until we see an int
                    // token type 2 == int
                    if (current.type != TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    // Special case to ignore the screen number if present, as this VTT parser just uses its
                    // index
                    if ((cursor + 1) < tokens.Count && tokens[cursor + 1].type == TokenType.Newline)
                    {
                        cursor++;
                        continue;
                    }

                    string timestamp = ConsumeUntil(TokenType.Space);
                    int timestampMillis = this.ParseTimestamp(timestamp, '.', ':');

                    if (timestampMillis == -1)
                    {
                        Node errorNode = new Node(
                            NodeKind.Unknown,
                            $"Failed to parse start timestamp: {timestamp}"
                        );
                        nodes.Add(errorNode);

                        // Don't change the mode, if we're expecting a start timestamp, we should go until we find one.
                        continue;
                    }

                    // node kind 2 == TimestampStart
                    Node node = new Node(NodeKind.TimestampStart, timestampMillis);
                    nodes.Add(node);

                    // Move to expecting the arrow
                    mode = Mode.ExpectingArrow;
                    continue;
                }

                if (mode == Mode.ExpectingArrow)
                {
                    // Go until we see a hyphen
                    if (current.type != TokenType.Arrow)
                    {
                        cursor++;
                        continue;
                    }

                    cursor++;

                    // Move to expecting the second timestamp, even if we didn't see an arrow
                    // to prevent infinite loops.
                    mode = Mode.ExpectingEndTimestamp;
                    continue;
                }

                if (mode == Mode.ExpectingEndTimestamp)
                {
                    // Go until we see an int
                    if (current.type != TokenType.Number)
                    {
                        cursor++;
                        continue;
                    }

                    string timestamp = ConsumeUntil(TokenType.Newline, TokenType.Space);
                    int timestampMillis = this.ParseTimestamp(timestamp, '.', ':');

                    if (timestampMillis == -1)
                    {
                        Node errorNode = new Node(
                            NodeKind.Unknown,
                            $"Failed to parse end timestamp: {timestamp}"
                        );
                        nodes.Add(errorNode);

                        // Don't change the mode, if we're expecting a start timestamp, we should go until we find one.
                        continue;
                    }

                    // node kind 4 == TimestampEnd
                    Node node = new Node(NodeKind.TimestampEnd, timestampMillis);
                    nodes.Add(node);

                    // Move to expecting the arrow
                    mode = Mode.ExpectingTextParameters;
                    continue;
                }

                if (mode == Mode.ExpectingTextParameters)
                {
                    if (current.type == TokenType.Newline)
                    {
                        cursor++;
                        mode = Mode.ExpectingTextContent;
                        continue;
                    }

                    string allParameters = "";

                    while (cursor < tokens.Count && tokens[cursor].type != TokenType.Newline)
                    {
                        allParameters += tokens[cursor].value;
                        cursor++;
                    }

                    string[] stringParameters = allParameters.Split(' ');
                    Dictionary<string, string> parameters = new Dictionary<string, string>();

                    foreach (string parameter in stringParameters)
                    {
                        string[] parts = parameter.Split(':');
                        if (parts.Length != 2)
                            continue;

                        string name = parts[0];
                        string value = parts[1];

                        parameters.Add(name, value);
                    }

                    Node node = new Node(NodeKind.TextParameters, parameters);
                    nodes.Add(node);
                    mode = Mode.ExpectingTextContent;
                    continue;
                }

                // Expect the text contents
                if (mode == Mode.ExpectingTextContent)
                {
                    int tCursor = cursor;
                    string textContents = "";

                    while (
                        tCursor < tokens.Count
                        && tokens.ElementAt(tCursor).type != TokenType.DoubleNewline
                    )
                    {
                        VttLexer.Token tCurrent = tokens.ElementAt(tCursor);
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
                    mode = Mode.ExpectingStartTimestamp;
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
