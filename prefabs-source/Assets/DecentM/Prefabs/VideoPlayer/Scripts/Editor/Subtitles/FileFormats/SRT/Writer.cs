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
            int index = -1;

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
                    sb.AppendLine(index.ToString());
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
