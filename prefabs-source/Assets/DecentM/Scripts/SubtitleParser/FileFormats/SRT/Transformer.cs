using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DecentM.Subtitles.Srt
{
    public class Transformer
    {
        public List<SubtitleScreen> ToSubtitleScreens(AstParser.Ast ast)
        {
            List<SubtitleScreen> screens = new List<SubtitleScreen>();

            int index = 0;
            int timestampStart = 0;
            int timestampEnd = 0;
            string text = "";

            for (int i = 0; i < ast.nodes.Count; i++)
            {
                AstParser.Node node = ast.nodes.ElementAtOrDefault(i);

                if (object.Equals(node, null))
                {
                    continue;
                }

                if (node.kind == AstParser.NodeKind.ScreenIndex)
                {
                    int.TryParse((string)node.value, out index);
                }

                if (node.kind == AstParser.NodeKind.TimestampStart)
                {
                    timestampStart = (int)node.value;
                }

                if (node.kind == AstParser.NodeKind.TimestampEnd)
                {
                    timestampEnd = (int)node.value;
                }

                if (node.kind == AstParser.NodeKind.TextContents)
                {
                    text = (string)node.value;

                    SubtitleScreen screen = new SubtitleScreen(index, timestampStart, timestampEnd, text);

                    screens.Add(screen);
                }
            }

            return screens;
        }

        public List<Instruction> ToInstructions(AstParser.Ast ast)
        {
            List<SubtitleScreen> screens = this.ToSubtitleScreens(ast);

            return Instruction.FromScreens(screens);
        }
    }
}
