using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerJoinLeaveProvider : UdonSharpBehaviour
{
    public NotificationSystem notifications;
    public Toggle toggle;

    public Sprite joinIcon;
    public Sprite leaveIcon;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!this.toggle.isOn)
            return;

        string displayName = "Someone";
        if (player != null && player.IsValid())
            displayName = player.displayName;

        this.notifications.SendNotification(this.joinIcon, $"{displayName} joined");
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!this.toggle.isOn)
            return;

        string displayName = "Someone";
        if (player != null && player.IsValid())
            displayName = player.displayName;

        this.notifications.SendNotification(this.leaveIcon, $"{displayName} left");
    }
}
