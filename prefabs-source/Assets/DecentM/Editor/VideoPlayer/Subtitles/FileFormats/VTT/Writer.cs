using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace DecentM.Prefabs.Subtitles.Vtt
{
    public class VttWriter : Writer
    {
        public VttWriter(Ast ast) : base(ast) { }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int index = -1;

            sb.Append("WEBVTT\n\n");
            sb.Append("NOTE\nFile written by DecentM's subtitle compiler\n\n");

            for (int i = 0; i < ast.nodes.Count; i++)
            {
                Node node = ast.nodes.ElementAtOrDefault(i);

                if (object.Equals(node, null))
                {
                    continue;
                }

                if (node.kind == NodeKind.TimestampStart)
                {
                    index++;
                    sb.Append(index.ToString());
                    sb.Append('\n');

                    int timestamp = (int)node.value;
                    TimeSpan t = TimeSpan.FromMilliseconds(timestamp);
                    string stringTimestamp = string.Format(
                        "{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds
                    );
                    sb.Append($"{stringTimestamp} --> ");
                }

                if (node.kind == NodeKind.TimestampEnd)
                {
                    int timestamp = (int)node.value;
                    TimeSpan t = TimeSpan.FromMilliseconds(timestamp);
                    string stringTimestamp = string.Format(
                        "{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds
                    );
                    sb.Append(stringTimestamp);
                    sb.Append('\n');
                }

                if (node.kind == NodeKind.TextContents)
                {
                    sb.Append($"{node.value}\n\n");
                }

                if (node.kind == NodeKind.Unknown)
                {
                    sb.Append("NOTE Unknown text encountered:\n");
                    sb.Append(node.value.ToString().Replace('\n', ' '));
                    sb.Append("\n\n");
                }
            }

            return sb.ToString();
        }
    }
}
