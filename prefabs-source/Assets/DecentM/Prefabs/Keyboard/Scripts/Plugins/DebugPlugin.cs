using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.Keyboard.Plugins
{
    public class DebugPlugin : KeyboardPlugin
    {
        public TextMeshProUGUI logGui;

        private void Log(params string[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                this.Log("(received empty log input)");
                return;
            }

            for (int i = 0; i < messages.Length; i++)
            {
                if (messages[i] == null)
                    continue;

                this.logGui.text += $"{messages[i]} ";
            }

            this.logGui.text += "\n";
        }

        protected override void _Start()
        {
            this.logGui.text = "";
            this.Log(nameof(_Start));
        }

        protected override void OnDebugLog(string message)
        {
            this.Log(message);
        }

        protected override void OnKeyPressDown(KeyboardKey key)
        {
            this.Log(nameof(OnKeyPressDown), key.name);
        }

        protected override void OnKeyPressUp(KeyboardKey key)
        {
            this.Log(nameof(OnKeyPressUp), key.name);
        }

        protected override void OnSymbolEntry(string symbol)
        {
            this.Log(nameof(OnSymbolEntry), symbol);
        }

        protected override void OnBackspace()
        {
            this.Log(nameof(OnBackspace));
        }

        protected override void OnShiftStateChange(bool state, KeyboardLayout layout)
        {
            this.Log(nameof(OnShiftStateChange), state.ToString(), layout.name);
        }

        protected override void OnCtrlStateChange(bool state, KeyboardLayout layout)
        {
            this.Log(nameof(OnCtrlStateChange), state.ToString(), layout.name);
        }

        protected override void OnAltGrStateChange(bool state, KeyboardLayout layout)
        {
            this.Log(nameof(OnAltGrStateChange), state.ToString(), layout.name);
        }

        protected override void OnCommandClear()
        {
            this.Log(nameof(OnCommandClear));
        }

        protected override void OnCommandClipboardCut()
        {
            this.Log(nameof(OnCommandClipboardCut));
        }

        protected override void OnCommandClipboardPaste()
        {
            this.Log(nameof(OnCommandClipboardPaste));
        }

        protected override void OnCommandClipboardCopy()
        {
            this.Log(nameof(OnCommandClipboardCopy));
        }

        protected override void OnCommandUndo()
        {
            this.Log(nameof(OnCommandUndo));
        }

        protected override void OnCommandRedo()
        {
            this.Log(nameof(OnCommandRedo));
        }

        protected override void OnCommandQuit()
        {
            this.Log(nameof(OnCommandQuit));
        }

        protected override void OnCommandCloseTab()
        {
            this.Log(nameof(OnCommandCloseTab));
        }

        protected override void OnCommandRefresh()
        {
            this.Log(nameof(OnCommandRefresh));
        }

        protected override void OnCommandOpen()
        {
            this.Log(nameof(OnCommandOpen));
        }

        protected override void OnCommandPrint()
        {
            this.Log(nameof(OnCommandPrint));
        }

        protected override void OnCommandSelectAll()
        {
            this.Log(nameof(OnCommandSelectAll));
        }

        protected override void OnCommandSave()
        {
            this.Log(nameof(OnCommandSave));
        }

        protected override void OnCommandDuplicate()
        {
            this.Log(nameof(OnCommandDuplicate));
        }

        protected override void OnCommandSearch()
        {
            this.Log(nameof(OnCommandSearch));
        }

        protected override void OnCommandHistory()
        {
            this.Log(nameof(OnCommandHistory));
        }

        protected override void OnCommandNew()
        {
            this.Log(nameof(OnCommandNew));
        }

        protected override void OnCommandMap()
        {
            this.Log(nameof(OnCommandMap));
        }

        protected override void OnCommandQuickAction()
        {
            this.Log(nameof(OnCommandQuickAction));
        }

        protected override void OnCommandNext()
        {
            this.Log(nameof(OnCommandNext));
        }

        protected override void OnInputSubmit(string input)
        {
            this.Log(nameof(OnInputSubmit), input);
        }
    }
}
