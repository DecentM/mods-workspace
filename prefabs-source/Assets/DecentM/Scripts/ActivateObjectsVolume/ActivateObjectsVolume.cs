
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ActivateObjectsVolume : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("A list of GameObjects to toggle. They will always have the same state")]
    public GameObject[] targets;
    [Tooltip("If true, the trigger will enable/disable targets when other players enter/exit it")]
    public bool global = false;

    [Header("LibDecentM")]
    [Tooltip("The Permissions object from LibDecentM")]
    public Permissions permissions;
    [Tooltip("If checked, the list will function as a whitelist, otherwise it will function as a blacklist")]
    public bool isWhitelist = false;
    [Tooltip("If checked, only the instance master can use this trigger, and the player list will be ignored")]
    public bool masterOnly = false;
    [Tooltip("A list of players who can (or cannot) use this trigger")]
    public PlayerList playerList;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.IsValid())
        {
            return;
        }

        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (player != localPlayer && !this.global)
        {
            return;
        }

        bool isAllowed = this.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList ? this.playerList.players : new string[0]);

        if (!isAllowed)
        {
            return;
        }

        for (int i = 0; i < this.targets.Length; i++)
        {
            this.targets[i].SetActive(true);
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.IsValid())
        {
            return;
        }

        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (player != localPlayer && !this.global)
        {
            return;
        }

        bool isAllowed = this.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList ? this.playerList.players : new string[0]);

        if (!isAllowed)
        {
            return;
        }

        for (int i = 0; i < this.targets.Length; i++)
        {
            this.targets[i].SetActive(false);
        }
    }
}
