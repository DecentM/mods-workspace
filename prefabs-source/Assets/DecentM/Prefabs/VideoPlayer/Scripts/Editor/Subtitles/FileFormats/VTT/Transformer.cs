using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DecentM.TextProcessing;

namespace DecentM.Subtitles.Vtt
{
    public class VttTransformer : Transformer<NodeKind>
    {
        public VttTransformer(Ast<NodeKind> ast)
        {
            this.input = ast;
        }

        public override Transformer<NodeKind> LigaturiseArabicText()
        {
            Ast<NodeKind> newAst = new Ast<NodeKind>(NodeKind.VttAst);

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
