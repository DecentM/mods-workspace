using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        public override string ToString()
        {
            return $"{(int)this.type} {this.timestamp} {this.value.Replace('\n', 'ª')}";
        }
    }
}
