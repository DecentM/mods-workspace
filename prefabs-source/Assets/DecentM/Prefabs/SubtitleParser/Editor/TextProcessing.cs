using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Web;
using System.Net;

namespace DecentM.Subtitles
{
    public class TextProcessing
    {
        private string input = "";

        public TextProcessing(string input)
        {
            this.input = input;
        }

        public string GetResult()
        {
            return this.input;
        }

        public TextProcessing CRLFToLF()
        {
            this.input = input.Replace("\r\n", "\n");
            return this;
        }

        public TextProcessing AsciiToUTF8()
        {
            this.input = Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(input));
            return this;
        }

        public TextProcessing ResolveHTMLEntities()
        {
            this.input = input
                .Replace("&nbsp;", " ");

            return this;
        }
    }
}
