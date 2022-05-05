using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DecentM.Subtitles
{
    public enum InstructionType
    {
        Unknown = 0,
        RenderText = 1,
        Clear = 2,
    }

    public struct Instruction
    {
        public Instruction(InstructionType type, int timestamp, string value)
        {
            this.type = type;
            this.timestamp = timestamp;
            this.value = value;
        }

        public InstructionType type;
        public int timestamp;
        public string value;

        public const char NewlineDelimeter = '˟';

        public override string ToString()
        {
            return $"{(int)this.type} {this.timestamp} {this.value.Replace('\n', NewlineDelimeter)}";
        }

        public SubtitleScreen ToSubtitleScreen(int index, int length)
        {
            return new SubtitleScreen(index, this.timestamp, this.timestamp + length, this.value);
        }

        public static List<Instruction> FromScreens(List<SubtitleScreen> screens)
        {
            List<Instruction> instructions = new List<Instruction>();

            for (int i = 0; i < screens.Count; i++)
            {
                SubtitleScreen screen = screens.ElementAtOrDefault(i);
                SubtitleScreen nextScreen = screens.ElementAtOrDefault(i + 1);

                // If the current screen is somehow null, we just skip it
                if (object.Equals(screen, null))
                {
                    continue;
                }

                SubtitleScreenInstructions screenInstructions = screen.ToInstructions();
                instructions.Add(screenInstructions.start);

                // If this is the last screen, we need to write the last clear instruction.
                if (object.Equals(nextScreen, null))
                {
                    instructions.Add(screenInstructions.end);
                    continue;
                }

                // Skip creating a clear instruction if the next write instruction is less than 200ms away, to prevent
                // the subtitles from blinking. The in-game renderer runs every 10th of a second, so every change shorter
                // than that would result in a 100ms gap anyway.
                if (nextScreen.startTimestamp > screen.endTimestamp + 200)
                {
                    instructions.Add(screenInstructions.end);
                    continue;
                }
            }

            return instructions;
        }
    }

    // Intermediate struct used while converting screens to instructions
    public struct SubtitleScreenInstructions
    {
        public SubtitleScreenInstructions(SubtitleScreen screen)
        {
            this.start = new Instruction(InstructionType.RenderText, screen.startTimestamp, screen.text);
            this.end = new Instruction(InstructionType.Clear, screen.endTimestamp, "");
        }

        public Instruction start;
        public Instruction end;
    }

    // A screen is an instance of subtitle text displayed. It contains all information
    // needed to render a section of subtitles
    public struct SubtitleScreen
    {
        public SubtitleScreen(int index, int startTimestamp, int endTimestamp, string text)
        {
            this.index = index;
            this.startTimestamp = startTimestamp;
            this.endTimestamp = endTimestamp;
            this.text = text;
        }

        public int index;
        public int startTimestamp;
        public int endTimestamp;
        public string text;

        public SubtitleScreenInstructions ToInstructions()
        {
            return new SubtitleScreenInstructions(this);
        }
    }
}
