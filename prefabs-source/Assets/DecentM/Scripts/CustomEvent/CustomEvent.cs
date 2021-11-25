
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CustomEvent : UdonSharpBehaviour
{
    [Tooltip("The UdonBehaviour to send the custom event to")]
    public UdonBehaviour target;

    [Header("Settings")]
    [Tooltip("The name of the event to send / public function name to call")]
    public string eventName;
    [Tooltip("If true, the custom event will be sent to everyone in the room")]
    public bool global = false;

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;
    [Tooltip("If checked, the list will function as a whitelist, otherwise it will function as a blacklist")]
    public bool isWhitelist = false;
    [Tooltip("If checked, only the instance master can use this trigger, and the player list will be ignored")]
    public bool masterOnly = false;
    [Tooltip("A list of players who can (or cannot) use this trigger")]
    public PlayerList playerList;

    public override void Interact()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        bool isAllowed = this.lib.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList);

        if (!isAllowed || this.eventName == null || this.target == null)
        {
            return;
        }

        if (this.global)
        {
            this.SendGlobal();
        }
        else
        {
            this.SendLocal();
        }
    }

    private void SendGlobal()
    {
        this.target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, this.eventName);
    }

    private void SendLocal()
    {
        this.target.SendCustomEvent(this.eventName);
    }
}
