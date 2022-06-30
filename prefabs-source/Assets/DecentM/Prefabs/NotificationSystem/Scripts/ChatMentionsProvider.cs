using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using DecentM.Chat;
using DecentM.Pubsub;

using DecentM.Chat.Plugins;

namespace DecentM.Notifications.Providers
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class ChatMentionsProvider : ChatPlugin
    {
        public NotificationSystem notifications;
        public Toggle toggle;
        public Sprite icon;

        protected override void OnMessageAdded(string id, object[] message)
        {
            if (!this.toggle.isOn || message == null)
                return;

            int senderId = this.system.messages.GetSenderId(id);
            VRCPlayerApi sender = VRCPlayerApi.GetPlayerById(senderId);

            if (sender == null || !sender.IsValid())
                return;
            if (sender == Networking.LocalPlayer)
                return;

            string text = this.system.messages.GetMessageText(id);

            if (text.ToLower().Contains(Networking.LocalPlayer.displayName.ToLower()))
            {
                this.notifications.SendNotification(this.icon, $"{sender.displayName}\n{text}");
            }
        }
    }
}
