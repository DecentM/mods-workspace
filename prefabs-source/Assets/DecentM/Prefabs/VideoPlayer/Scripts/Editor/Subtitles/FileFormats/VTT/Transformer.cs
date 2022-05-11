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
        private Ast<NodeKind> ArabicTextLigaturiserTransform(Ast<NodeKind> input)
        {
            Ast<NodeKind> newAst = new Ast<NodeKind>(NodeKind.VttAst);

            foreach (Node<NodeKind> node in input.nodes)
            {
                Node<NodeKind> newNode = node;

                if (node.kind == NodeKind.TextContents)
                {
                    newNode = new Node<NodeKind>(node.kind, ArabicText.ConvertLigatures((string)node.value));
                }

                newAst.nodes.Add(newNode);
            }

            return newAst;
        }

        public override Transformer<NodeKind> LigaturiseArabicText()
        {
            this.AddTransform(this.ArabicTextLigaturiserTransform);

            return this;
        }
    }
}
