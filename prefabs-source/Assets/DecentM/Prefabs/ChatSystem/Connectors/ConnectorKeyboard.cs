using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM.Keyboard;

namespace DecentM.Chat.Connectors
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ConnectorKeyboard : UdonSharpBehaviour
    {
        public ChatSystem system;
        public KeyboardEvents events;

        void Start()
        {
            this.events.Subscribe(this);
        }

        private void HandleSymbolEntry()
        {
            this.system.OnPlayerTyping();
        }

        private void HandleSubmit(string message)
        {
            this.system.OnSendMessage(0, message);
        }

        private string OnKeyboardEvent_name;
        private object[] OnKeyboardEvent_data;
        public void OnKeyboardEvent()
        {
            switch (OnKeyboardEvent_name)
            {
                case nameof(this.events.OnSymbolEntry):
                    this.HandleSymbolEntry();
                    break;

                case nameof(this.events.OnInputSubmit):
                    this.HandleSubmit((string)OnKeyboardEvent_data[0]);
                    break;

                default:
                    break;
            }
        }
    }
}
