using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DecentM.TextProcessing;

namespace DecentM.Subtitles
{
    public sealed class Transformer
    {
        private List<Func<Ast, Ast>> transforms = new List<Func<Ast, Ast>>();

        private Ast ArabicTextLigaturiserTransform(Ast input)
        {
            Ast newAst = new Ast(NodeKind.Root);

            foreach (Node node in input.nodes)
            {
                Node newNode = node;

                if (node.kind == NodeKind.TextContents)
                {
                    newNode = new Node(node.kind, ArabicText.ConvertLigatures((string)node.value));
                }

                newAst.nodes.Add(newNode);
            }

            return newAst;
        }

        private void AddTransform(Func<Ast, Ast> transform)
        {
            if (transforms.Contains(transform)) return;

            transforms.Add(transform);
        }

        #region Public API

        public Transformer LigaturiseArabicText()
        {
            this.AddTransform(this.ArabicTextLigaturiserTransform);

            return this;
        }

        public Ast Transform(Ast input)
        {
            Ast result = input;

            foreach (Func<Ast, Ast> transformer in transforms)
            {
                result = transformer(result);
            }

            return result;
        }

        #endregion
    }
}
