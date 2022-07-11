using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using TMPro;

namespace DecentM.Chat.Plugins
{
    public class DebugPlugin : ChatPlugin
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

        protected override void OnMessageAdded(string id, object[] message)
        {
            this.Log(nameof(OnMessageAdded), id);
        }

        protected override void OnMessageChanged(string id, object[] message)
        {
            this.Log(nameof(OnMessageChanged), id);
        }

        protected override void OnMessageDeleted(string id, object[] message)
        {
            this.Log(nameof(OnMessageDeleted), id);
        }

        protected override void OnPlayerTypingStart(int playerId)
        {
            this.Log(nameof(OnPlayerTypingStart), playerId.ToString());
        }

        protected override void OnPlayerTypingStop(int playerId)
        {
            this.Log(nameof(OnPlayerTypingStop), playerId.ToString());
        }

        protected override void OnProfilePictureChange(int playerId)
        {
            this.Log(nameof(OnProfilePictureChange), playerId.ToString());
        }

        protected override void OnPlayerPresent(int playerId)
        {
            this.Log(nameof(OnPlayerPresent), playerId.ToString());
        }

        protected override void OnPlayerAway(int playerId)
        {
            this.Log(nameof(OnPlayerAway), playerId.ToString());
        }
    }
}
