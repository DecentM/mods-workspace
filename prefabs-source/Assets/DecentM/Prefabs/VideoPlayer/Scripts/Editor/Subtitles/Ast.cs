using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecentM.Subtitles
{
    public enum NodeKind
    {
        Unknown,
        Root,
        Header,
        Comment,
        ScreenIndex,
        TimestampStart,
        TimestampArrow,
        TimestampEnd,
        TextContents,
        TextParameters,
    }

    public struct Node
    {
        public Node(NodeKind kind, string value)
        {
            this.kind = kind;
            this.value = value;
        }

        public Node(NodeKind kind, int value)
        {
            this.kind = kind;
            this.value = value;
        }

        public Node(NodeKind kind, Dictionary<string, string> value)
        {
            this.kind = kind;
            this.value = value;
        }

        public readonly NodeKind kind;
        public readonly object value;
    }

    public struct Ast
    {
        public Ast(NodeKind kind, List<Node> nodes)
        {
            this.nodes = nodes;
            this.kind = kind;
        }

        public Ast(NodeKind kind)
        {
            this.nodes = new List<Node>();
            this.kind = kind;
        }

        public readonly NodeKind kind;
        public readonly List<Node> nodes;

        public List<Node> GetUnknowns()
        {
            return this.nodes.Where(node => node.kind == NodeKind.Unknown).ToList();
        }

        public string Dump()
        {
            string result = "";

            foreach (Node node in this.nodes)
            {
                result += $"{node.kind.ToString()}\n";
                result += $"{node.value.ToString()}\n";
                result += $"==================\n";
            }

            return result;
        }
    }
}
