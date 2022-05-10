using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DecentM.Subtitles.Vtt
{
    public class VttWriter : Writer<VttParser, NodeKind, VttLexer, TokenType>
    {
        public override string ToString(Ast<NodeKind> ast)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("WEBVTT\n\n");
            sb.Append("NOTE\nFile written by DecentM's subtitle compiler\n\n");

            for (int i = 0; i < ast.nodes.Count; i++)
            {
                Node<NodeKind> node = ast.nodes.ElementAtOrDefault(i);

                if (object.Equals(node, null))
                {
                    continue;
                }

                if (node.kind == NodeKind.TimestampStart)
                {
                    sb.Append($"{node.value} --> ");
                }

                if (node.kind == NodeKind.TimestampEnd)
                {
                    sb.Append($"{node.value}\n");
                }

                if (node.kind == NodeKind.TextContents)
                {
                    sb.Append($"{node.value}\n\n");
                }
            }

            return sb.ToString();
        }

        public List<SubtitleScreen> ToSubtitleScreens(Ast<NodeKind> ast)
        {
            List<SubtitleScreen> screens = new List<SubtitleScreen>();

            int index = 0;
            int timestampStart = 0;
            int timestampEnd = 0;
            string text = "";

            for (int i = 0; i < ast.nodes.Count; i++)
            {
                Node<NodeKind> node = ast.nodes.ElementAtOrDefault(i);

                if (object.Equals(node, null))
                {
                    continue;
                }

                if (node.kind == NodeKind.TimestampStart)
                {
                    timestampStart = (int)node.value;
                }

                if (node.kind == NodeKind.TimestampEnd)
                {
                    timestampEnd = (int)node.value;
                }

                if (node.kind == NodeKind.TextContents)
                {
                    text = (string)node.value;
                    index++;

                    SubtitleScreen screen = new SubtitleScreen(index, timestampStart, timestampEnd, text);

                    screens.Add(screen);
                }
            }

            return screens;
        }

        public List<Instruction> ToInstructions(Ast<NodeKind> ast)
        {
            List<SubtitleScreen> screens = this.ToSubtitleScreens(ast);

            return Instruction.FromScreens(screens);
        }
    }
}
