using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DecentM.Subtitles.Vsi
{
    public class VsiWriter : Writer
    {
        public VsiWriter(Ast ast) : base(ast) { }

        public override string ToString()
        {
            List<SubtitleScreen> screens = SubtitleScreen.FromNodes(this.ast.nodes);
            List<Instruction> instructions = Instruction.FromScreens(screens);

            StringBuilder sb = new StringBuilder();

            foreach (Instruction instruction in instructions)
            {
                sb.Append(instruction.ToString());
                sb.Append('\n');
            }

            return sb.ToString();
        }
    }
}
