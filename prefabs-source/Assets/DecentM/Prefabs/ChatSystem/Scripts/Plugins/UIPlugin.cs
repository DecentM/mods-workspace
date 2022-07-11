using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using TMPro;

namespace DecentM.Chat.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIPlugin : ChatPlugin
    {
        protected override void OnMessageAdded(string id, object[] message)
        {
            this.Rerender();
        }

        protected override void OnMessageChanged(string id, object[] message)
        {
            this.Rerender();
        }

        protected override void OnMessageDeleted(string id, object[] message)
        {
            this.Rerender();
        }

        public TextMeshProUGUI[] slots;

        private void Rerender()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                TextMeshProUGUI slot = slots[i];

                if (slot == null)
                    continue;

                slot.text = "";
            }

            object[] messageIds = this.system.messages.messages.Keys;
            int renderCount = Mathf.Min(this.slots.Length, messageIds.Length);

            for (int i = 0; i < renderCount; i++)
            {
                string text = this.system.messages.GetMessageText((string)messageIds[i]);
                TextMeshProUGUI slot = slots[renderCount - 1 - i];

                if (string.IsNullOrEmpty(text) || slot == null)
                    continue;

                slot.text = text;
            }
        }
    }
}
