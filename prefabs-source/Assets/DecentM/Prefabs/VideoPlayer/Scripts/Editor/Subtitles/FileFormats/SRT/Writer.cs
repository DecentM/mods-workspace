using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DecentM.Subtitles.Srt
{
    public class SrtWriter : Writer
    {
        public SrtWriter(Ast ast) : base(ast) { }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < ast.nodes.Count; i++)
            {
                Node node = ast.nodes.ElementAtOrDefault(i);

                if (object.Equals(node, null))
                {
                    continue;
                }

                if (node.kind == NodeKind.ScreenIndex)
                {
                    sb.Append($"{node.value}\n");
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
    }
}
