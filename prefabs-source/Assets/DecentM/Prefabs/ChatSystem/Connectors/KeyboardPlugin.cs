using UdonSharp;
using VRC.SDKBase;

using DecentM.Keyboard;
using DecentM.Pubsub;

namespace DecentM.Chat.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class KeyboardPlugin : ChatPlugin
    {
        private void HandleSymbolEntry()
        {
            this.events.OnPlayerTypingStart(Networking.LocalPlayer.playerId);
        }

        private void HandleSubmit(string message)
        {
            this.system.OnSendMessage(message);
        }
    }
}
