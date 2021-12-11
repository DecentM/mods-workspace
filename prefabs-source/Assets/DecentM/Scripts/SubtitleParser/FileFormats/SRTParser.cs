
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SRTParser : UdonSharpBehaviour
{
    public TextAsset srtFile;
    public LibDecentM lib;

    private object[] CreateToken(int type, object value)
    {
        object[] token = new object[2];

        /** Index map
         * 0 - type
         * 1 - value
         **/

        /**
         * TextType - a token that could be a control char, or just
         * part of the subtitles text
         * 
         * Token types:
         * 0 - unknown / lexing error
         * 1 - char
         * 2 - number (a number from 0-9)
         * 3 - hyphen (-)
         * 4 - sideways caret (>)
         * 5 - colon (:)
         * 6 - comma (,)
         * 7 - newline (/n or /r/n)
         * 8 - space
         **/

        token[0] = type;
        token[1] = value;

        return token;
    }

    public object[][] Lex(string text)
    {
        int cursor = 0;
        object[][] tokens = new object[0][];
        
        while (cursor < text.Length)
        {
            char current = text[cursor];

            // Hyphens are in arrows, and text
            if (current == '-')
            {
                object[] token = this.CreateToken(3, "-");

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            // Sideways carets are in arrows, and text
            if (current == '>')
            {
                object[] token = this.CreateToken(4, ">");

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            // int.Parse throws and error if passed a string that we can't catch because Udon, so we have a bit of a nightmare here
            if (current == '0')
            {
                object[] token = this.CreateToken(2, 0);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '1')
            {
                object[] token = this.CreateToken(2, 1);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '2')
            {
                object[] token = this.CreateToken(2, 2);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '3')
            {
                object[] token = this.CreateToken(2, 3);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '4')
            {
                object[] token = this.CreateToken(2, 4);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '5')
            {
                object[] token = this.CreateToken(2, 5);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '6')
            {
                object[] token = this.CreateToken(2, 6);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '7')
            {
                object[] token = this.CreateToken(2, 7);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '8')
            {
                object[] token = this.CreateToken(2, 8);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '9')
            {
                object[] token = this.CreateToken(2, 9);

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == ':')
            {
                object[] token = this.CreateToken(5, ":");

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == ',')
            {
                object[] token = this.CreateToken(6, ",");

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            if (current == '\n')
            {
                object[] token = this.CreateToken(7, "\n");

                tokens = this.lib.arrayTools.Push(tokens, token);

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
                object[] token = this.CreateToken(8, " ");

                tokens = this.lib.arrayTools.Push(tokens, token);

                cursor++;
                continue;
            }

            // If we're down here, that means we have a char, so we just treat it as part of
            // the subtitle text
            object[] charToken = this.CreateToken(1, current);

            tokens = this.lib.arrayTools.Push(tokens, charToken);

            cursor++;
            continue;
        }

        return tokens;
    }

    private object[] CreateNode(int kind, object value)
    {
        object[] node = new object[2];

        /** Index map
         * 0 - kind
         * 1 - value
         **/

        /**
         * Kinds:
         * 0 - unknown / parsing error
         * 1 - ScreenIndex
         * 2 - TimestampStart
         * 3 - TimestampArrow
         * 4 - TimestampEnd
         * 5 - TextContents
         * 6 - ScreenEnd
         **/

        node[0] = kind;
        node[1] = value;

        return node;
    }

    private int ParseTimestampFromIndex(object[][] tokens, int index)
    {
        int tCursor = index;
        object[] tCurrent = tokens[tCursor];
        string timestamp = "";

        // Keep going until we see a space
        while ((int)tCurrent[0] != 8)
        {
            tCursor++;
            tCurrent = tokens[tCursor];
            timestamp = $"{timestamp}{tCurrent[1]}";
        }

        // timestamp format: 00:05:00,400
        // hours:minutes:seconds,milliseconds
        string[] parts = timestamp.Split(',');
        string time = parts[0];
        int millis = int.Parse(parts[1]);

        string[] timeParts = time.Split(':');
        int hours = int.Parse(timeParts[0]);
        int minutes = int.Parse(timeParts[1]);
        int seconds = int.Parse(timeParts[2]);

        // Add values to get the value in millis
        return millis + (seconds + 1000) + (minutes + 60 * 1000) + (hours + 60 * 60 * 1000);
    }

    public object[][] Parse(object[][] tokens)
    {
        object[][] nodes = new object[0][];
        int cursor = 0;
        int mode = 1; // Node kind, to check where we currently are

        while (cursor < tokens.Length)
        {
            object[] current = tokens[cursor];
            int tokenType = (int) current[0];
            var tokenValue = current[1];

            // If we're expecting ScreenIndex
            if (mode == 1)
            {
                // Go until we see an int
                // token type 2 == int
                if (tokenType != 2)
                {
                    cursor++;
                    continue;
                }

                // If the current token is not a number, go down until we find one
                // token type 2 == int
                object[] node = this.CreateNode(1, tokenValue);
                nodes = this.lib.arrayTools.Push(nodes, node);

                cursor++;
                // Move to expecting a start timestamp
                mode++;
                continue;
            }

            // If we're in the start timestamp
            if (mode == 2)
            {
                // Go until we see an int
                // token type 2 == int
                if (tokenType != 2)
                {
                    cursor++;
                    continue;
                }

                int timestampMillis = this.ParseTimestampFromIndex(tokens, cursor);

                // node kind 2 == TimestampStart
                object[] node = this.CreateNode(2, timestampMillis);
                nodes = this.lib.arrayTools.Push(nodes, node);

                // Skip the timestamp + a space
                cursor = cursor + 12;
                // Move to expecting the arrow
                mode++;
                continue;
            }

            // Expect the arrow
            if (mode == 3)
            {
                // Go until we see a hyphen
                if (tokenType != 3)
                {
                    cursor++;
                    continue;
                }

                int tCursor = cursor;
                object[] tCurrent = tokens[tCursor];
                string body = "";

                // Keep going until we see a space
                while ((int) tCurrent[0] != 8)
                {
                    body = $"{body}{tCurrent[1]}";
                    tCursor++;
                    tCurrent = tokens[tCursor];
                }

                if (body == "-->")
                {
                    // node kind 3 == TimestampArrow
                    object[] node = this.CreateNode(3, tokenValue);
                    nodes = this.lib.arrayTools.Push(nodes, node);

                    cursor = cursor + tCursor;

                    // Move to expecting the second timestamp
                    mode++;
                }
                continue;
            }

            // Expect the end timestamp
            if (mode == 4)
            {
                // Go until we see an int
                // token type 2 == int
                if (tokenType != 2)
                {
                    cursor++;
                    continue;
                }

                int timestampMillis = this.ParseTimestampFromIndex(tokens, cursor);

                // node kind 4 == TimestampEnd
                object[] node = this.CreateNode(4, timestampMillis);
                nodes = this.lib.arrayTools.Push(nodes, node);

                // Skip the timestamp + a space
                cursor = cursor + 12;
                // Move to expecting the arrow
                mode++;
                continue;
            }

            // Expect the text contents
            if (mode == 5)
            {
                int tCursor = cursor;
                bool prevCharIsNewline = false;
                string textContents = "";

                while (true)
                {
                    object[] tCurrent = tokens[tCursor];

                    // If we've seen two consecutive newlines, it's the end of the text contents part,
                    // and we need to return to expecting the next subtitle screen
                    // tokenType 7 == newline
                    if (tokenType == 7)
                    {
                        if (prevCharIsNewline)
                        {
                            break;
                        }

                        textContents = $"{textContents}{(string)tokenValue}";
                        prevCharIsNewline = true;
                    }
                    else
                    {
                        textContents = $"{textContents}{(string)tokenValue}";
                        prevCharIsNewline = false;
                    }
                }

                object[] node = this.CreateNode(5, textContents);
                nodes = this.lib.arrayTools.Push(nodes, node);

                cursor = tCursor;
                mode = 1;
                continue;
            }
        }

        return nodes;
    }
}
