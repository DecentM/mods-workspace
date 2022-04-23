using UdonSharp;
using DecentM.Keyboard;
using DecentM.Pubsub;

namespace DecentM.Chat.Connectors
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class ConnectorKeyboard : PubsubSubscriber<KeyboardEvent>
    {
        public ChatSystem system;

        private void HandleSymbolEntry()
        {
            this.system.OnPlayerTyping();
        }

        private void HandleSubmit(string message)
        {
            this.system.OnSendMessage(0, message);
        }

        protected override void OnPubsubEvent(KeyboardEvent name, object[] data)
        {
            switch (name)
            {
                case KeyboardEvent.OnSymbolEntry:
                    this.HandleSymbolEntry();
                    break;

                case KeyboardEvent.OnInputSubmit:
                    this.HandleSubmit((string)data[0]);
                    break;

                default:
                    break;
            }
        }
    }
}
