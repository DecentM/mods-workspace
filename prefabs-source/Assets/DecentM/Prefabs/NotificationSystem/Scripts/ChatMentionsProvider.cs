using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using DecentM.Chat;
using DecentM.Pubsub;

namespace DecentM.Notifications.Providers
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class ChatMentionsProvider : PubsubSubscriber<ChatEvent>
    {
        public NotificationSystem notifications;
        public Toggle toggle;
        public Sprite icon;

        protected override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                case ChatEvent.OnMessageAdded:
                    this.HandleMessageAdded((ChatMessage)data[0]);
                    break;
                default:
                    break;
            }
        }

        private void HandleMessageAdded(ChatMessage message)
        {
            if (!this.toggle.isOn || message == null) return;

            VRCPlayerApi sender = VRCPlayerApi.GetPlayerById(message.senderId);

            if (sender == null || !sender.IsValid()) return;
            if (sender == Networking.LocalPlayer) return;

            if (message.message.ToLower().Contains(Networking.LocalPlayer.displayName.ToLower()))
            {
                this.notifications.SendNotification(this.icon, $"{sender.displayName}\n{message.message}");
            }
        }
    }
}
