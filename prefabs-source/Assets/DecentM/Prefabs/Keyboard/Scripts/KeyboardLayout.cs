using System;
using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KeyboardLayout : UdonSharpBehaviour
    {
        public string layoutName = "";

        public KeyboardEvents events;

        [HideInInspector]
        public KeyboardSystem system;

        private KeyboardKey[] allKeys;
        private KeyboardKey[][] keysBySymbolIndex;

        private void Start()
        {
            if (this.layoutName == "")
                this.layoutName = this.name;

            this.allKeys = this.GetComponentsInChildren<KeyboardKey>();
            this.AttachSelfToKeys();
            this.keysBySymbolIndex = new KeyboardKey[0][];
            this.IndexKeysBySymbol();
        }

        private int FindSymbolIndex(string symbol)
        {
            for (int i = 0; i < this.keysBySymbolIndex.Length; i++)
            {
                KeyboardKey[] keys = this.keysBySymbolIndex[i];

                if (keys == null || keys.Length == 0)
                    continue;

                if (keys[0].primarySymbol == symbol)
                    return i;
            }

            return -1;
        }

        private void IndexKeysBySymbol()
        {
            foreach (KeyboardKey key in this.allKeys)
            {
                int index = this.FindSymbolIndex(key.primarySymbol);

                // If the index doesn't exist, create it
                if (index == -1)
                {
                    KeyboardKey[][] tmpList = new KeyboardKey[this.keysBySymbolIndex.Length + 1][];
                    Array.Copy(this.keysBySymbolIndex, tmpList, this.keysBySymbolIndex.Length);
                    tmpList[tmpList.Length - 1] = new KeyboardKey[] { key };
                    this.keysBySymbolIndex = tmpList;
                    key.SetProgramVariable("symbolIndex", tmpList.Length - 1);
                    continue;
                }

                KeyboardKey[] keys = this.keysBySymbolIndex[index];

                if (keys == null)
                    continue;

                // If the symbol already exists, add it to the existing index
                KeyboardKey[] tmp = new KeyboardKey[keys.Length + 1];
                Array.Copy(keys, tmp, keys.Length);
                tmp[tmp.Length - 1] = key;
                this.keysBySymbolIndex[index] = tmp;

                key.SetProgramVariable("symbolIndex", index);
            }
        }

        public KeyboardKey[] GetKeysBySymbolIndex(int index)
        {
            if (index < 0 || index >= this.keysBySymbolIndex.Length)
                return null;

            return this.keysBySymbolIndex[index];
        }

        private int GetIndexBySymbol(string symbol)
        {
            for (int i = 0; i < this.keysBySymbolIndex.Length; i++)
            {
                KeyboardKey[] keys = this.keysBySymbolIndex[i];

                if (keys == null || keys.Length == 0)
                    continue;
                if (keys[0].primarySymbol == symbol)
                    return i;
            }

            return -1;
        }

        public KeyboardKey[] GetKeysByPrimarySymbol(string symbol)
        {
            int index = this.GetIndexBySymbol(symbol);

            return this.GetKeysBySymbolIndex(index);
        }

        private void AttachSelfToKeys()
        {
            foreach (KeyboardKey key in this.allKeys)
            {
                key.SetProgramVariable(nameof(key.keyboardLayout), this);
            }
        }

        private int shiftModifier = 1;
        private int ctrlModifier = 2;
        private int altGrModifier = 4;

        private int currentModifiers = 0;

        private void AddModifier(int modifier)
        {
            this.currentModifiers |= modifier;
        }

        private void RemoveModifier(int modifier)
        {
            this.currentModifiers &= ~modifier;
        }

        public void SetState(int modifier, bool state)
        {
            if (state)
                this.AddModifier(modifier);
            else
                this.RemoveModifier(modifier);
        }

        private bool HasModifier(int state)
        {
            return (this.currentModifiers & state) == state;
        }

        private void HandleCtrlCommand(KeyboardKey key)
        {
            switch (key.primarySymbol)
            {
                case "BACKSPACE":
                    this.events.OnCommandClear();
                    break;
                case "X":
                    this.events.OnCommandClipboardCut();
                    break;
                case "C":
                    this.events.OnCommandClipboardCopy();
                    break;
                case "V":
                    this.events.OnCommandClipboardPaste();
                    break;
                case "Z":
                    this.events.OnCommandUndo();
                    break;
                case "Y":
                    this.events.OnCommandRedo();
                    break;
                case "Q":
                    this.events.OnCommandQuit();
                    break;
                case "W":
                    this.events.OnCommandCloseTab();
                    break;
                case "R":
                    this.events.OnCommandRefresh();
                    break;
                case "O":
                    this.events.OnCommandOpen();
                    break;
                case "P":
                    this.events.OnCommandPrint();
                    break;
                case "A":
                    this.events.OnCommandSelectAll();
                    break;
                case "S":
                    this.events.OnCommandSave();
                    break;
                case "D":
                    this.events.OnCommandDuplicate();
                    break;
                case "F":
                    this.events.OnCommandSearch();
                    break;
                case "H":
                    this.events.OnCommandHistory();
                    break;
                case "N":
                    this.events.OnCommandNew();
                    break;
                case "M":
                    this.events.OnCommandMap();
                    break;
                case "SPACE":
                    this.events.OnCommandQuickAction();
                    break;
                case "TAB":
                    this.events.OnCommandNext();
                    break;
                default:
                    break;
            }
        }

        public void HandleKeyPress(KeyboardKey key)
        {
            if (this.HasModifier(this.ctrlModifier) && key.primarySymbol != "CTRL")
            {
                this.HandleCtrlCommand(key);
                this.SetState(this.ctrlModifier, false);
                this.events.OnCtrlStateChange(false, this);
                return;
            }

            switch (key.primarySymbol)
            {
                case "SHIFT":
                {
                    bool newState = !this.HasModifier(this.shiftModifier);
                    this.SetState(this.shiftModifier, newState);
                    this.events.OnShiftStateChange(newState, this);
                    break;
                }

                case "CTRL":
                {
                    bool newState = !this.HasModifier(this.ctrlModifier);
                    this.SetState(this.ctrlModifier, newState);
                    this.events.OnCtrlStateChange(newState, this);
                    break;
                }

                case "ALTGR":
                {
                    bool newState = !this.HasModifier(this.altGrModifier);
                    this.SetState(this.altGrModifier, newState);
                    this.events.OnAltGrStateChange(newState, this);
                    break;
                }

                case "TAB":
                    this.events.OnSymbolEntry("\t");
                    break;

                case "SPACE":
                    this.events.OnSymbolEntry(" ");
                    break;

                case "BACKSPACE":
                    this.events.OnBackspace();
                    break;

                case "ENTER":
                    this.events.OnSymbolEntry("\n");
                    break;

                // Default means it's not a special key and we need to do symbol lookup based on the current shift/ctrl state
                default:
                    string symbol = key.primarySymbol.ToLower();

                    if (this.HasModifier(this.shiftModifier))
                    {
                        // If the key has one symbol, shift means uppercase
                        if (key.symbolCount == 1)
                            symbol = key.primarySymbol.ToUpper();
                        // If it has two or more, shift means the secondary symbol
                        else
                            symbol = key.secondarySymbol;

                        this.SetState(this.shiftModifier, false);
                        this.events.OnShiftStateChange(false, this);
                    }

                    if (this.HasModifier(this.altGrModifier))
                    {
                        symbol = key.tertiarySymbol;

                        this.SetState(this.altGrModifier, false);
                        this.events.OnAltGrStateChange(false, this);
                    }

                    // If there's no symbol to send, we ignore the keypress
                    if (symbol == null || symbol == "")
                        return;

                    this.events.OnSymbolEntry(symbol);
                    break;
            }
        }
    }
}
