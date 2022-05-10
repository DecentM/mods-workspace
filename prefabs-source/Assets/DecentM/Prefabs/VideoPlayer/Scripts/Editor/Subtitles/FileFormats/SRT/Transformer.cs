using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DecentM.TextProcessing;

namespace DecentM.Subtitles.Srt
{
    public class SrtTransformer : Transformer<NodeKind>
    {
        public SrtTransformer(Ast<NodeKind> ast)
        {
            this.input = ast;
        }

        public override Transformer<NodeKind> LigaturiseArabicText()
        {
            Ast<NodeKind> newAst = new Ast<NodeKind>(NodeKind.SubtitleParserAst);

            foreach (Node<NodeKind> node in this.input.nodes)
            {
                Node<NodeKind> newNode = node;

                if (node.kind == NodeKind.TextContents)
                {
                    newNode = new Node<NodeKind>(node.kind, ArabicText.ConvertLigatures((string)node.value));
                }

                newAst.nodes.Add(newNode);
            }

            this.input = newAst;

            return this;
        }
    }
}
