using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using DecentM.Chat;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ChatMentionsProvider : UdonSharpBehaviour
{
    public NotificationSystem notifications;
    public Toggle toggle;
    public Sprite icon;

    [Space]
    public ChatEvents events; 

    void Start()
    {
        this.events.Subscribe(this);
    }

    private string OnChatEvent_name;
    private object OnChatEvent_data;
    public void OnChatEvent()
    {
        this.HandleChatEvent(OnChatEvent_name, OnChatEvent_data);
    }

    private void HandleChatEvent(string name, object data)
    {
        switch (name)
        {
            case nameof(this.events.OnMessageAdded):
                this.HandleMessageAdded((ChatMessage) data);
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
