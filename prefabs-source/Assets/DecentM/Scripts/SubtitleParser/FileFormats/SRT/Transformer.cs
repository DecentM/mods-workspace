using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.Subtitles.Srt
{
    public class Transformer
    {
        public List<Instruction> ToInstructions(AstParser.Ast ast)
        {
            List<Instruction> instructions = new List<Instruction>();

            int index = 0;
            int timestampStart = 0;
            int timestampEnd = 0;
            string text = "";

            ast.nodes.ForEach(delegate (AstParser.Node node)
            {
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

                    Instruction startInstruction = new Instruction(InstructionType.RenderText, timestampStart, text);
                    Instruction endInstruction = new Instruction(InstructionType.Clear, timestampEnd, "");
                    instructions.Add(startInstruction);
                    instructions.Add(endInstruction);
                }
            });

            return instructions;
        }
    }
}
