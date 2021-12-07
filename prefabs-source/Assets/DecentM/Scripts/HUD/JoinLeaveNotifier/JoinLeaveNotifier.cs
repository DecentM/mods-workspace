
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class JoinLeaveNotifier : UdonSharpBehaviour
{
    [Header("Notification Settings")]
    [Tooltip("If checked, join notifications will be shown")]
    public bool showJoinNotifications = true;
    [Tooltip("If checked, leave notifications will be shown")]
    public bool showLeaveNotifications = true;
    [Tooltip("The icon to show for joining players")]
    public Sprite joinIcon;
    [Tooltip("The icon to show for leaving players")]
    public Sprite leaveIcon;
    [Space]
    [Tooltip("The notification image slot")]
    public Image imageSlot;

    public Text usernameSlot;
    public Text actionSlot;
    public Animator animator;

    public void SetLeave()
    {
        this.actionSlot.text = "Left";
        this.imageSlot.sprite = this.leaveIcon;
    }

    public void SetJoin()
    {
        this.actionSlot.text = "Joined";
        this.imageSlot.sprite = this.joinIcon;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!this.showJoinNotifications)
        {
            return;
        }

        this.SetJoin();
        this.usernameSlot.text = player.displayName;
        this.animator.SetTrigger("PlayJoinMessage");
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!this.showLeaveNotifications)
        {
            return;
        }

        this.SetLeave();
        this.usernameSlot.text = player.displayName;
        this.animator.SetTrigger("PlayJoinMessage");
    }
}
