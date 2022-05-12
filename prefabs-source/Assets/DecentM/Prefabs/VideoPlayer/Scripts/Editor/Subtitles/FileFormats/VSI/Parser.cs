using System;
using System.Collections.Generic;
using System.Linq;

namespace DecentM.Subtitles.Vsi
{
    public class VsiParser : Parser<VsiLexer, TokenType>
    {
        private enum Mode
        {
            ExpectingInstructionLine,
            ExpectingNewline,
        }

        public override Ast Parse(List<VsiLexer.Token> tokens)
        {
            List<Node> nodes = new List<Node>();
            int cursor = 0;

            int GetNextTimestamp()
            {
                int tCursor = cursor + 1;
                VsiLexer.Token tCurrent;

                while (tCursor < tokens.Count)
                {
                    tCurrent = tokens[tCursor];

                    if (tCurrent.type != TokenType.InstructionLine)
                    {
                        tCursor++;
                        continue;
                    }

                    Instruction instruction = Instruction.FromInstructionLine((string)tCurrent.value);
                    return instruction.timestamp;
                }

                return -1;
            }

            int currentIndex = -1;

            while (cursor < tokens.Count)
            {
                VsiLexer.Token current = tokens.ElementAt(cursor);

                if (current.type != TokenType.InstructionLine)
                {
                    cursor++;
                    continue;
                }

                currentIndex++;

                Instruction instruction = Instruction.FromInstructionLine((string)current.value);

                if (instruction.type == InstructionType.Clear)
                {
                    cursor++;
                    continue;
                }

                // If we reached the end and still have an instruction, we just default to ending it in 10 seconds
                int nextTimestamp = GetNextTimestamp();
                if (nextTimestamp == -1)
                {
                    nextTimestamp = instruction.timestamp + 10000;
                }

                SubtitleScreen screen = instruction.ToScreen(currentIndex, nextTimestamp - instruction.timestamp);

                nodes.AddRange(screen.ToNodes());

                cursor++;
                continue;
            }

            return new Ast(NodeKind.Root, nodes);
        }
    }
}
