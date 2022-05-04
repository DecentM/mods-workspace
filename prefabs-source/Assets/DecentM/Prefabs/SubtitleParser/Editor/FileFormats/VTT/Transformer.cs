using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Vtt
{
    public class Transformer
    {
        public List<SubtitleScreen> ToSubtitleScreens(Parser.Ast ast)
        {
            List<SubtitleScreen> screens = new List<SubtitleScreen>();

            int index = 0;
            int timestampStart = 0;
            int timestampEnd = 0;
            string text = "";

            for (int i = 0; i < ast.nodes.Count; i++)
            {
                Parser.Node node = ast.nodes.ElementAtOrDefault(i);

                if (object.Equals(node, null))
                {
                    continue;
                }

                /* if (node.kind == Parser.NodeKind.ScreenIndex)
                {
                    int.TryParse((string)node.value, out index);
                } */

                if (node.kind == Parser.NodeKind.TimestampStart)
                {
                    timestampStart = (int)node.value;
                }

                if (node.kind == Parser.NodeKind.TimestampEnd)
                {
                    timestampEnd = (int)node.value;
                }

                if (node.kind == Parser.NodeKind.TextContents)
                {
                    text = (string)node.value;
                    index++;

                    SubtitleScreen screen = new SubtitleScreen(index, timestampStart, timestampEnd, text);

                    screens.Add(screen);
                }
            }

            return screens;
        }

        public List<Instruction> ToInstructions(Parser.Ast ast)
        {
            List<SubtitleScreen> screens = this.ToSubtitleScreens(ast);

            return Instruction.FromScreens(screens);
        }
    }
}
