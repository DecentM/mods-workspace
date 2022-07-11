using UdonSharp;
using VRC.SDKBase;

using DecentM.Keyboard;
using DecentM.Pubsub;

namespace DecentM.Chat.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class ChatKeyboardPlugin : ChatPlugin
    {
        public void OnSymbolEntry()
        {
            this.events.OnPlayerTypingStart(Networking.LocalPlayer.playerId);
        }

        public void OnSubmit(string message)
        {
            this.system.OnSendMessage(message);
        }
    }
}
