using System.Text;

namespace DecentM.TextProcessing
{
    public class TextProcessor
    {
        private string input = "";

        public TextProcessor(string input)
        {
            this.input = input;
        }

        public string GetResult()
        {
            return this.input;
        }

        public TextProcessor CRLFToLF()
        {
            this.input = input.Replace("\r\n", "\n");
            return this;
        }

        public TextProcessor AsciiToUTF8()
        {
            this.input = Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(this.input));
            return this;
        }

        public TextProcessor LigaturiseArabicText()
        {
            this.input = ArabicText.ConvertLigatures(this.input);
            return this;
        }

        public TextProcessor ResolveHTMLEntities()
        {
            this.input = input
                .Replace("&nbsp;", " ")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "\'")
                .Replace("&cent;", "¢")
                .Replace("&pound;", "£")
                .Replace("&yen;", "¥")
                .Replace("&euro;", "€")
                .Replace("&copy;", "©")
                .Replace("&reg;", "®");

            return this;
        }
    }
}
