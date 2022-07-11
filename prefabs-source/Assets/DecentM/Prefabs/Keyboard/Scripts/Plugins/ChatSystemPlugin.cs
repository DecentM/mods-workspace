using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using DecentM.Chat.Plugins;

namespace DecentM.Keyboard.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChatSystemPlugin : KeyboardPlugin
    {
        public ChatKeyboardPlugin chatPlugin;

        protected override void OnSymbolEntry(string symbol)
        {
            this.chatPlugin.OnSymbolEntry();
        }

        protected override void OnInputSubmit(string input)
        {
            this.chatPlugin.OnSubmit(input);
        }
    }
}
