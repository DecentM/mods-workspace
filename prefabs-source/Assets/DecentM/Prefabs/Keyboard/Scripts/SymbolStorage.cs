using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/*
 * I'm so sorry.
 * Mainly for myself for having to write this lookup table,
 * but since VRChat doesn't support Dictionaries, or Lists,
 * there's no way to find which key on the keyboard belongs
 * to which letter without searching through them.
 *
 * Since the keyboard needs to be able to address each
 * key individually in a performant way, I needed a way to
 * find a key based on its primary symbol in O(1) time, while
 * supporting non-ascii characters. Therefore, ta-dah!
 */

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SymbolStorage : UdonSharpBehaviour
    {
        // A list of possible symbols that can be typed with the keyboard system
        // The order doesn't matter, the indices are used to map keys to their
        // values for easy lookup
        [HideInInspector]
        private string[] symbols = new string[]
        {
            "`",
            "-",
            "=",
            "[",
            "]",
            "\\",
            ";",
            "'",
            ",",
            ".",
            "/",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "0",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "ENTER",
            "ESC",
            "CTRL",
            "SHIFT",
            "TAB",
            "BACKSPACE",
        };

        public int GetIndexBySymbol(string symbol)
        {
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i] == symbol)
                    return i;
            }

            return -1;
        }

        public string GetSymbolByIndex(int index)
        {
            // Return null if out of range
            if (index >= symbols.Length || index < 0)
                return null;

            return this.symbols[index];
        }

        public bool SymbolExists(string symbol)
        {
            return this.GetIndexBySymbol(symbol) != -1;
        }

        public int count
        {
            get { return this.symbols.Length; }
        }
    }
}
