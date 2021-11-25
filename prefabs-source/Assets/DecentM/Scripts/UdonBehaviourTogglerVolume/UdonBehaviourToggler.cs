
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UdonBehaviourToggler : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("The UdonBehaviour to toggle")]
    public UdonBehaviour udonBehaviour;

    [Header("Settings")]
    [Tooltip("If checked, the behaviour will be toggled for everyone")]
    public bool global;

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;
    [Tooltip("If checked, the list will function as a whitelist, otherwise it will function as a blacklist")]
    public bool isWhitelist = false;
    [Tooltip("If checked, only the instance master can use this trigger, and the player list will be ignored")]
    public bool masterOnly = false;
    [Tooltip("A list of players who can (or cannot) use this trigger")]
    public PlayerList playerList;

    private void Start()
    {
        MeshRenderer mesh = this.GetComponent<MeshRenderer>();

        this.lib.debugging.ApplyToMeshRenderer(mesh);

        // Disable the UdonBehaviour by default
        this.udonBehaviour.enabled = false;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        // We don't do anything if we're already following someone or the player isn't valid
        if (!player.IsValid())
        {
            return;
        }

        bool isAllowed = this.lib.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList);

        if (!isAllowed)
        {
            return;
        }

        if (this.global)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.ToggleOn));
        } else
        {
            this.ToggleOn();
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        // No reason to unfollow a player if we're not already following them
        if (!player.IsValid())
        {
            return;
        }

        if (this.global)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.ToggleOff));
        }
        else
        {
            this.ToggleOff();
        }
    }

    public void ToggleOn()
    {
        this.udonBehaviour.enabled = true;
    }

    public void ToggleOff()
    {
        this.udonBehaviour.enabled = false;
    }
}
